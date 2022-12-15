using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues;

internal sealed class StorageLargeMessageQueue<TMessage>
    : StorageLargeMessageQueueBase<TMessage>
{
    public StorageLargeMessageQueue(AzureLargeMessageQueueOptions options)
        : base(options)
    {
    }
}

internal sealed class AzureLargeMessageQueue<TMessage, TDiscriminator>
    : StorageLargeMessageQueueBase<TMessage>
    , IMessageQueue<TMessage, TDiscriminator>
{
    public AzureLargeMessageQueue(AzureLargeMessageQueueOptions options)
        : base(options)
    {
    }
}
