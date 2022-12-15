namespace OneBeyond.Studio.Infrastructure.RabbitMQ.Options;

public sealed class RabbitPubSubMessageQueueReceiverOptions : RabbitPubSubMessageQueueOptions
{
    public string? QueueNameSuffix { get; set; }
}
