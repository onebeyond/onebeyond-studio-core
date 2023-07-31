using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using EnsureThat;

namespace OneBeyond.Studio.Infrastructure.Azure.Storage;

public static class BlobContainer
{
    public static async Task<BlobContainerClient> GetOrCreateAsync(
        string? storageName,
        string? connectionString,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        var blobContainerClient = string.IsNullOrWhiteSpace(storageName)
            ? GetConnectionStringClient(connectionString!, queueName)
            : GetAzureIdentityClient(storageName!, queueName);

        await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return blobContainerClient;
    }

    private static BlobContainerClient GetConnectionStringClient(
        string connectionString,
        string containerName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(connectionString, nameof(connectionString));
        EnsureArg.IsNotNullOrWhiteSpace(containerName, nameof(containerName));

        return new BlobContainerClient(connectionString, containerName);
    }

    private static BlobContainerClient GetAzureIdentityClient(
       string storageName,
       string containerName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(storageName, nameof(storageName));
        EnsureArg.IsNotNullOrWhiteSpace(containerName, nameof(containerName));

        return new BlobContainerClient(
           new Uri($"https://{storageName}.blob.core.windows.net/{containerName}"),
           new DefaultAzureCredential());
    }
}
