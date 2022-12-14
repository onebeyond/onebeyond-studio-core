using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using EnsureThat;

namespace OneBeyond.Studio.Infrastructure.Azure.Storage;

public static class BlobContainer
{
    public static async Task<BlobContainerClient> GetOrCreateAsync(
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
}
