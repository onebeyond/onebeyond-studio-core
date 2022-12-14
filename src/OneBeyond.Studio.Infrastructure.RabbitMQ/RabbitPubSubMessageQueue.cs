using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.RabbitMQ.Options;

namespace OneBeyond.Studio.Infrastructure.RabbitMQ;

internal sealed class RabbitPubSubMessageQueue<TMessage>
    : RabbitPubSubMessageQueueBase<TMessage>
{
    public RabbitPubSubMessageQueue(RabbitPubSubMessageQueueOptions options)
        : base(options)
    {
    }
}

internal sealed class RabbitPubSubMessageQueue<TMessage, TDiscriminator>
    : RabbitPubSubMessageQueueBase<TMessage>
    , IMessageQueue<TMessage, TDiscriminator>
{
    public RabbitPubSubMessageQueue(RabbitPubSubMessageQueueOptions options)
        : base(options)
    {
    }
}
