using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues;

internal sealed class StorageMessageQueue<TMessage>
    : StorageMessageQueueBase<TMessage>
{
    public StorageMessageQueue(AzureMessageQueueOptions options)
        : base(options)
    {
    }
}

internal sealed class AzureMessageQueue<TMessage, TDiscriminator>
    : StorageMessageQueueBase<TMessage>
    , IMessageQueue<TMessage, TDiscriminator>
{
    public AzureMessageQueue(AzureMessageQueueOptions options)
        : base(options)
    {
    }
}
