using System;

namespace OneBeyond.Studio.Infrastructure.RabbitMQ.Options;

public class RabbitPubSubMessageQueueOptions
{
    public Uri? Connection { get; set; }
    public string? ExchangeName { get; set; }
}
