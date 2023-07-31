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
        return string.IsNullOrWhiteSpace(storageName)
            ? await GetOrCreateConnectionStringAsync(connectionString!, queueName, cancellationToken).ConfigureAwait(false)
            : await GetOrCreateManagedIdentityAsync(storageName!, queueName, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<BlobContainerClient> GetOrCreateConnectionStringAsync(
        string connectionString,
        string containerName,
        CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNullOrWhiteSpace(connectionString, nameof(connectionString));
        EnsureArg.IsNotNullOrWhiteSpace(containerName, nameof(containerName));

        var blobContainerClient = new BlobContainerClient(connectionString, containerName);

        await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return blobContainerClient;
    }

    private static async Task<BlobContainerClient> GetOrCreateManagedIdentityAsync(
       string storageName,
       string containerName,
       CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNullOrWhiteSpace(storageName, nameof(storageName));
        EnsureArg.IsNotNullOrWhiteSpace(containerName, nameof(containerName));

        var blobContainerClient = new BlobContainerClient(
           new Uri($"https://{storageName}.blob.core.windows.net/{containerName}"),
           new DefaultAzureCredential());

        await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return blobContainerClient;
    }
}
