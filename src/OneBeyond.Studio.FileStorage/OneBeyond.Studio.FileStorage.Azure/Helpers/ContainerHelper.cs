using System;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Nito.AsyncEx;
using OneBeyond.Studio.FileStorage.Azure.Exceptions;
using OneBeyond.Studio.FileStorage.Azure.Options;
using OneBeyond.Studio.FileStorage.Domain;

namespace OneBeyond.Studio.FileStorage.Azure.Helpers;

internal static class ContainerHelper
{
    private static readonly Regex _containerCharacterRegex = new(@"^[a-z0-9-]*$", RegexOptions.Compiled);
    private static readonly Regex _containerFullValidityRegex = new(@"^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", RegexOptions.Compiled);

    public static AsyncLazy<BlobContainerClient> CreateBlobContainerClient(
        AzureBaseStorageOptions options)
    {
        ValidateContainerName(options.ContainerName!);

        var blobServiceClient = new BlobServiceClient(options.ConnectionString);
        return new AsyncLazy<BlobContainerClient>(
            async () =>
            {
                var containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName!);
                await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
                return containerClient;
            },
            AsyncLazyFlags.RetryOnFailure);
    }

    public static Uri GetSharedAccessUriFromContainer(
        string blobFileId,
        CloudStorageAction action,
        BlobContainerClient containerClient,
        TimeSpan sharedAccessDuration)
    {
        BlobHelper.ValidateBlobName(blobFileId);

        var escapedBlobName = Uri.EscapeDataString(blobFileId);
        var blobClient = containerClient.GetBlobClient(escapedBlobName);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new AzureStorageException("BlobClient must be authorized with Shared Key credentials to create a service SAS.");
        }

        var startsOn = DateTime.UtcNow;

        var endsOn = startsOn.Add(sharedAccessDuration);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
            BlobName = blobClient.Name,
            StartsOn = startsOn,
            ExpiresOn = endsOn,
            Resource = "b",
        };

        switch (action)
        {
            case CloudStorageAction.Download:
                sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);
                break;
            case CloudStorageAction.Upload:
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Add | BlobContainerSasPermissions.Write | BlobContainerSasPermissions.Create);
                break;
            case CloudStorageAction.Delete:
                sasBuilder.SetPermissions(BlobAccountSasPermissions.Delete);
                break;
        }

        return blobClient.GenerateSasUri(sasBuilder);
    }

    //This is designed to improve information about what is and is not valid for a given azure container name.
    private static void ValidateContainerName(string containerName)
    {
        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new AzureStorageException("Container name cannot be empty.");
        }

        if (containerName.Length < 3 || containerName.Length > 63)
        {
            throw new AzureStorageException("Container name must be between 3 and 63 characters in length.");
        }

        if (!_containerCharacterRegex.IsMatch(containerName))
        {
            throw new AzureStorageException("Container name can only contain lowercase letters, numbers or hyphens.");
        }

        if (!_containerFullValidityRegex.IsMatch(containerName))
        {
            throw new AzureStorageException("Container name must start and end with a number or letter and cannot contain multiple hyphens in sequence.");
        }
    }
}
