using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues;

internal sealed class ServiceBusPubSubMessageQueue<TMessage>
    : ServiceBusPubSubMessageQueueBase<TMessage>
{
    public ServiceBusPubSubMessageQueue(AzureServicePubSubBusMessageQueueOptions options)
        : base(options)
    {
    }
}

internal sealed class ServiceBusPubSubMessageQueue<TMessage, TDiscriminator>
    : ServiceBusPubSubMessageQueueBase<TMessage>
    , IMessageQueue<TMessage, TDiscriminator>
{
    public ServiceBusPubSubMessageQueue(AzureServicePubSubBusMessageQueueOptions options)
        : base(options)
    {
    }
}
