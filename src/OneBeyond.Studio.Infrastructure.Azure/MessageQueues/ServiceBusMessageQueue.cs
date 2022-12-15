using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues;

internal sealed class ServiceBusMessageQueue<TMessage>
    : ServiceBusMessageQueueBase<TMessage>
{
    public ServiceBusMessageQueue(AzureServiceBusMessageQueueOptions options)
        : base(options)
    {
    }
}

internal sealed class ServiceBusMessageQueue<TMessage, TDiscriminator>
    : ServiceBusMessageQueueBase<TMessage>
    , IMessageQueue<TMessage, TDiscriminator>
{
    public ServiceBusMessageQueue(AzureServiceBusMessageQueueOptions options)
        : base(options)
    {
    }
}
