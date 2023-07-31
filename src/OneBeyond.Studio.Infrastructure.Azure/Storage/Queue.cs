using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Queues;
using EnsureThat;

namespace OneBeyond.Studio.Infrastructure.Azure.Storage;

public static class Queue
{
    public static async Task<QueueClient> GetOrCreateAsync(
        string? storageName,
        string? connectionString,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        var queueClient = string.IsNullOrWhiteSpace(storageName)
            ? GetConnectionStringClient(connectionString!, queueName)
            : GetAzureIdentityClient(storageName!, queueName);

        await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return queueClient;
    }

    private static QueueClient GetConnectionStringClient(
        string connectionString,
        string queueName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(connectionString, nameof(connectionString));
        EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));

        return new QueueClient(connectionString, queueName);
    }

    private static QueueClient GetAzureIdentityClient(
        string storageName,
        string queueName)
    {
        EnsureArg.IsNotNullOrWhiteSpace(storageName, nameof(storageName));
        EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));

        return new QueueClient(
            new Uri($"https://{storageName}.queue.core.windows.net/{queueName}"), 
            new DefaultAzureCredential());
    }
}
