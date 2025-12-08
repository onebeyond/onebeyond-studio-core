using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.Strings;
using RabbitMQ.Client;

namespace OneBeyond.Studio.Infrastructure.RabbitMQ;

internal sealed class RabbitMessageQueuePoisonSetup
{
    private readonly string _exchangeName;
    private readonly string _queueName;
    private readonly string _routingKey;

    private readonly IChannel _channel;

    public RabbitMessageQueuePoisonSetup(IChannel channel, string queueName, bool useQueueNameAsRoutingKey)
    {
        EnsureArg.IsNotNull(channel, nameof(channel));
        EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));

        _exchangeName = $"{queueName}-poison";
        _queueName = _exchangeName;
        _channel = channel;
        _routingKey = useQueueNameAsRoutingKey
            ? _queueName
            : string.Empty;
    }

    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(_queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(_queueName, _exchangeName, _routingKey, cancellationToken: cancellationToken);
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
