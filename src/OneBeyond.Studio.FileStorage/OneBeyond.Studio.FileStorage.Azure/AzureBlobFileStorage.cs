using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using EnsureThat;
using Nito.AsyncEx;
using OneBeyond.Studio.FileStorage.Azure.Exceptions;
using OneBeyond.Studio.FileStorage.Azure.Helpers;
using OneBeyond.Studio.FileStorage.Azure.Options;
using OneBeyond.Studio.FileStorage.Domain;
using OneBeyond.Studio.FileStorage.Domain.Entities;
using OneBeyond.Studio.FileStorage.Domain.Exceptions;
using OneBeyond.Studio.FileStorage.Domain.Models;
using OneBeyond.Studio.FileStorage.Domain.Options;

namespace OneBeyond.Studio.FileStorage.Azure;

internal sealed class AzureBlobFileStorage : FileStorageBase
{
    private const string BLOB_METADATA_FILE_NAME = "fileName";

    private readonly AsyncLazy<BlobContainerClient> _defaultBlobContainerClient;
    private readonly AzureBlobFileStorageOptions _fileStorageOptions;

    public AzureBlobFileStorage(
        MimeTypeValidationOptions mimeTypeValidationOptions,
        AzureBlobFileStorageOptions fileStorageOptions)
        : base(mimeTypeValidationOptions)
    {
        EnsureArg.IsNotNull(fileStorageOptions, nameof(fileStorageOptions));

        _fileStorageOptions = fileStorageOptions;
        _defaultBlobContainerClient = ContainerHelper.CreateBlobContainerClient(fileStorageOptions);
    }

    protected override async Task<Uri> DoGetFileUrlAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
    {
        if (_fileStorageOptions.SharedAccessDuration is null || _fileStorageOptions.SharedAccessDuration.Value <= TimeSpan.Zero)
        {
            throw new AzureStorageException("Unable to generate a file url, SharedAccessDurationis not set.");
        }

        var fileName = GetBlobName(fileId);

        cancellationToken.ThrowIfCancellationRequested();

        var containerClient = await _defaultBlobContainerClient.Task.ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return ContainerHelper.GetSharedAccessUriFromContainer(
            fileName,
            CloudStorageAction.Download,
            containerClient,
            _fileStorageOptions.SharedAccessDuration.Value);
    }

    protected override async Task DoUploadFileContentAsync(
        FileRecord fileRecord,
        Stream fileContent,
        CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(fileRecord.Id).ConfigureAwait(false);

        var uploadOptions = new BlobUploadOptions
        {
            Metadata = new Dictionary<string, string>
            {
                { BLOB_METADATA_FILE_NAME, fileRecord.Name }
            },
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = fileRecord.ContentType,
            }
        };

        await blobClient.UploadAsync(
            fileContent,
            uploadOptions,
            cancellationToken)
            .ConfigureAwait(false);
    }

    protected override async Task DoCopyFileAsync(
        FileRecord fromFileRecord,
        FileRecord toFileRecord,
        CancellationToken cancellationToken)
    {
        if (fromFileRecord.Id == toFileRecord.Id)
        {
            throw new FileStorageException("You cannot copy a file entry to itself");
        }

        var sourceBlobClient = await GetBlobClientAsync(fromFileRecord.Id).ConfigureAwait(false);

        if (!await sourceBlobClient.ExistsAsync(cancellationToken))
        {
            throw new Domain.Exceptions.FileNotFoundException(fromFileRecord.Id);
        }

        var destBlobClient = await GetBlobClientAsync(toFileRecord.Id).ConfigureAwait(false);

        BlobLeaseClient? lease = null;

        try
        {
            // Lease the source blob for the copy operation 
            // to prevent another client from modifying it.
            lease = sourceBlobClient.GetBlobLeaseClient();

            // Specifying -1 for the lease interval creates an infinite lease.
            await lease.AcquireAsync(TimeSpan.FromSeconds(-1), cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            // Start the copy operation.
            await destBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            // Get the destination blob's properties and display the copy status.
            var destProperties = await destBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (destProperties.Value.CopyStatus != CopyStatus.Success)
            {
                throw new FileStorageException($"File copy operation failed with copy status {destProperties.Value.CopyStatus}.");
            }

            destProperties.Value.Metadata[BLOB_METADATA_FILE_NAME] = toFileRecord.Name;

            await destBlobClient.SetMetadataAsync(destProperties.Value.Metadata, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RequestFailedException ex)
        {
            throw new FileStorageException("Failed to copy blobs.", ex);
        }
        finally
        {
            // Break the lease on the source blob.
            if (sourceBlobClient != null && lease != null)
            {
                // Update the source blob's properties.
                var sourceProperties = await sourceBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (sourceProperties.Value.LeaseState == LeaseState.Leased)
                {
                    // Break the lease on the source blob.
                    await lease.BreakAsync(new TimeSpan(0), cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    protected override async Task<Stream> DoDownloadFileContentAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(fileId).ConfigureAwait(false);

        return await blobClient.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    protected override async Task<FileContent> DoDownloadFileAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(fileId).ConfigureAwait(false);
        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var contentType = properties.Value.ContentType;

        var fileName = properties.Value.Metadata.TryGetValue(BLOB_METADATA_FILE_NAME, out var value)
            ? value
            : fileId.ToString();

        var cloudBlobStream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return new FileContent(
            fileName,
            contentType,
            cloudBlobStream);
    }

    protected override async Task DoDeleteFileContentAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(fileId).ConfigureAwait(false);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<BlobClient> GetBlobClientAsync(FileRecord.ID fileId)
    {
        var cloudBlobContainer = await _defaultBlobContainerClient.Task.ConfigureAwait(false);

        return cloudBlobContainer.GetBlobClient(GetBlobName(fileId));
    }

    private static string GetBlobName(FileRecord.ID fileId)
        => fileId.ToString().ToUpperInvariant();
}
