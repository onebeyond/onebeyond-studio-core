using System;

namespace OneBeyond.Studio.Infrastructure.RabbitMQ.Options;

public sealed class RabbitMessageQueueOptions
{
    public Uri? Connection { get; set; }
    public string? QueueName { get; set; }
}
