using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using EnsureThat;

namespace OneBeyond.Studio.Infrastructure.Azure.Storage;

public static class Queue
{
    public static async Task<QueueClient> GetOrCreateAsync(
        string connectionString,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNullOrWhiteSpace(connectionString, nameof(connectionString));
        EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));

        var queueClient = new QueueClient(connectionString, queueName);

        await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return queueClient;
    }
}
