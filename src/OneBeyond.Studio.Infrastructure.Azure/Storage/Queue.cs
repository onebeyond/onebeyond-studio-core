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
        return string.IsNullOrWhiteSpace(storageName)
            ? await GetOrCreateConnectionStringAsync(connectionString!, queueName, cancellationToken).ConfigureAwait(false)
            : await GetOrCreateAzureIdentityAsync(storageName!, queueName, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<QueueClient> GetOrCreateConnectionStringAsync(
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

    private static async Task<QueueClient> GetOrCreateAzureIdentityAsync(
        string storageName,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNullOrWhiteSpace(storageName, nameof(storageName));
        EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));

        var queueClient = new QueueClient(
            new Uri($"https://{storageName}.queue.core.windows.net/{queueName}"), 
            new DefaultAzureCredential());

        await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return queueClient;
    }
}
