using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using EnsureThat;
using Newtonsoft.Json;
using Nito.AsyncEx;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;
using OneBeyond.Studio.Infrastructure.Azure.Storage;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues;

internal abstract class StorageMessageQueueBase<TMessage> : IMessageQueue<TMessage>
{
    private readonly AsyncLazy<QueueClient> _messageQueue;

    protected StorageMessageQueueBase(AzureMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));

        options.EnsureIsValid();

        _messageQueue = new AsyncLazy<QueueClient>(
            () => Queue.GetOrCreateAsync(options.ResourceName, options.ConnectionString!, options.QueueName!),
            AsyncLazyFlags.RetryOnFailure);
    }

    public async Task PublishAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        var messageJson = JsonConvert.SerializeObject(message);

        var messageJsonBytes = System.Text.Encoding.UTF8.GetBytes(messageJson); //And line below required due to https://stackoverflow.com/questions/65041620/model-binding-issue-in-azure-function-after-switching-to-azure-storage-queues

        var messageAsBase64 = Convert.ToBase64String(messageJsonBytes);

        var messageQueue = await _messageQueue.Task.ConfigureAwait(false);

        await messageQueue.SendMessageAsync(
                messageAsBase64,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
