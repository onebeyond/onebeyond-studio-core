using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using EnsureThat;
using Nito.AsyncEx;
using OneBeyond.Studio.FileStorage.Azure.Exceptions;
using OneBeyond.Studio.FileStorage.Azure.Helpers;
using OneBeyond.Studio.FileStorage.Azure.Options;
using OneBeyond.Studio.FileStorage.Domain;

namespace OneBeyond.Studio.FileStorage.Azure;

internal sealed class AzureBlobCloudStorage : ICloudFileStorage
{
    private readonly AzureBlobCloudStorageOptions _fileStorageOptions;
    private readonly AsyncLazy<BlobContainerClient> _defaultBlobContainerClient;

    public AzureBlobCloudStorage(AzureBlobCloudStorageOptions fileStorageOptions)
    {
        EnsureArg.IsNotNull(fileStorageOptions, nameof(fileStorageOptions));

        if (fileStorageOptions.SharedAccessDuration is null
            || fileStorageOptions.SharedAccessDuration.Value <= TimeSpan.Zero)
        {
            throw new AzureStorageException("SharedAccessDuration is not set, the cloud storage provider will not be able to generate file urls.");
        }

        _fileStorageOptions = fileStorageOptions;
        _defaultBlobContainerClient = ContainerHelper.CreateBlobContainerClient(fileStorageOptions);
    }

    public Task<Uri> GetDownloadUrlAsync(string fileId, CancellationToken cancellationToken)
        => GetSasUriAsync(fileId, CloudStorageAction.Download, cancellationToken);

    public Task<Uri> GetUploadUrlAsync(string fileId, CancellationToken cancellationToken)
        => GetSasUriAsync(fileId, CloudStorageAction.Upload, cancellationToken);

    public Task<Uri> GetDeleteUrlAsync(string fileId, CancellationToken cancellationToken)
        => GetSasUriAsync(fileId, CloudStorageAction.Delete, cancellationToken);

    private async Task<Uri> GetSasUriAsync(string blobFileId, CloudStorageAction action, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNullOrWhiteSpace(blobFileId, nameof(blobFileId));

        cancellationToken.ThrowIfCancellationRequested();

        var containerClient = await _defaultBlobContainerClient.Task.ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        return ContainerHelper.GetSharedAccessUriFromContainer(
            blobFileId,
            action,
            containerClient,
            _fileStorageOptions.SharedAccessDuration!.Value);
    }
}
