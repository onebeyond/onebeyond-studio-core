using System.Collections.Generic;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.Strings;
using RabbitMQ.Client;

namespace OneBeyond.Studio.Infrastructure.RabbitMQ;

internal sealed class RabbitMessageQueuePoisonSetup
{
    private readonly string _exchangeName;
    private readonly string _queueName;
    private readonly string _routingKey;

    public RabbitMessageQueuePoisonSetup(IModel channel, string queueName, bool useQueueNameAsRoutingKey)
    {
        EnsureArg.IsNotNull(channel, nameof(channel));
        EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));

        _exchangeName = $"{queueName}-poison";
        _queueName = _exchangeName;
        _routingKey = useQueueNameAsRoutingKey
            ? _queueName
            : string.Empty;

        channel.ExchangeDeclare(
            _exchangeName,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        channel.QueueDeclare(
            _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        channel.QueueBind(_queueName, _exchangeName, _routingKey);
    }

    public Dictionary<string, object> GetRoutingArguments()
        => _routingKey.IsNullOrEmpty()
            ? new Dictionary<string, object>
            {
                    { "x-dead-letter-exchange", _exchangeName }
            }
            : new Dictionary<string, object>
            {
                    { "x-dead-letter-exchange", _exchangeName },
                    { "x-dead-letter-routing-key", _routingKey }
            };
}
