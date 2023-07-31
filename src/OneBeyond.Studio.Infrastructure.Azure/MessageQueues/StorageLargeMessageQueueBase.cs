using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using EnsureThat;
using Newtonsoft.Json;
using Nito.AsyncEx;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;
using OneBeyond.Studio.Infrastructure.Azure.Storage;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues;

internal abstract class StorageLargeMessageQueueBase<TMessage> : IMessageQueue<TMessage>
{
    private readonly AsyncLazy<BlobContainerClient> _messageBlobContainer;
    private readonly AsyncLazy<QueueClient> _messageQueue;

    protected StorageLargeMessageQueueBase(AzureLargeMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));

        options.EnsureIsValid();

        _messageBlobContainer = new AsyncLazy<BlobContainerClient>(
            () => BlobContainer.GetOrCreateAsync(options.ResourceName, options.ConnectionString!, options.ContainerName!),
            AsyncLazyFlags.RetryOnFailure);

        _messageQueue = new AsyncLazy<QueueClient>(
            () => Queue.GetOrCreateAsync(options.ResourceName, options.ConnectionString!, options.QueueName!),
            AsyncLazyFlags.RetryOnFailure);
    }

    public async Task PublishAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        var messageJson = JsonConvert.SerializeObject(message);

        var messageJsonBytes = Encoding.UTF8.GetBytes(messageJson);

        var messageBlobContainer = await _messageBlobContainer.Task.ConfigureAwait(false);

        var messageBlobName = Guid.NewGuid().ToString();

        //Line below required due to https://stackoverflow.com/questions/65041620/model-binding-issue-in-azure-function-after-switching-to-azure-storage-queues
        var messageBlobNameAsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(messageBlobName));

        await messageBlobContainer.UploadBlobAsync(
                messageBlobName,
                new MemoryStream(messageJsonBytes),
                cancellationToken)
            .ConfigureAwait(false);

        var messageQueue = await _messageQueue.Task.ConfigureAwait(false);

        await messageQueue.SendMessageAsync(
                messageBlobNameAsBase64,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
