using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Newtonsoft.Json;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.RabbitMQ.Options;
using RabbitMQ.Client;

namespace OneBeyond.Studio.Infrastructure.RabbitMQ;

internal abstract class RabbitPubSubMessageQueueBase<TMessage>
    : IMessageQueue<TMessage>
    , IDisposable
{
    private readonly string _exchangeName;
    private readonly IConnection _connection;
    private IModel _channel;

    protected RabbitPubSubMessageQueueBase(RabbitPubSubMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));
        EnsureArg.IsNotNull(options.Connection, nameof(options.Connection));
        EnsureArg.IsNotNullOrWhiteSpace(options.ExchangeName, nameof(options.ExchangeName));

        var connectionFactory = new ConnectionFactory
        {
            Uri = options.Connection
        };

        _exchangeName = options.ExchangeName!;
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, true, false);
    }

    public void Dispose()
    {
        _channel.Close();
        _channel.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    public Task PublishAsync(TMessage message, CancellationToken cancellationToken)
    {
        var messageJson = JsonConvert.SerializeObject(message);
        var messageBytes = Encoding.UTF8.GetBytes(messageJson);

        lock (_connection)
        {
            if (_channel.IsClosed)
            {
                _channel = _connection.CreateModel();
            }

            _channel.BasicPublish(
                _exchangeName,
                string.Empty,
                default,
                messageBytes);
        }
        return Task.CompletedTask;
    }
}
