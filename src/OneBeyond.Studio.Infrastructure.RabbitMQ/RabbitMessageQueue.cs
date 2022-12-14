using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.RabbitMQ.Options;

namespace OneBeyond.Studio.Infrastructure.RabbitMQ;

internal sealed class RabbitMessageQueue<TMessage>
    : RabbitMessageQueueBase<TMessage>
{
    public RabbitMessageQueue(RabbitMessageQueueOptions options)
        : base(options)
    {
    }
}

internal sealed class RabbitMessageQueue<TMessage, TDiscriminator>
    : RabbitMessageQueueBase<TMessage>
    , IMessageQueue<TMessage, TDiscriminator>
{
    public RabbitMessageQueue(RabbitMessageQueueOptions options)
        : base(options)
    {
    }
}
