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

internal abstract class RabbitMessageQueueBase<TMessage>
    : IMessageQueue<TMessage>
    , IDisposable
{
    private readonly string _queueName;
    private readonly IConnection _connection;
    private IModel _channel;
    private readonly RabbitMessageQueuePoisonSetup _poisonSetup;

    protected RabbitMessageQueueBase(RabbitMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));
        EnsureArg.IsNotNull(options.Connection, nameof(options.Connection));
        EnsureArg.IsNotNullOrWhiteSpace(options.QueueName, nameof(options.QueueName));

        var connectionFactory = new ConnectionFactory
        {
            Uri = options.Connection
        };

        _queueName = options.QueueName!;
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _poisonSetup = new RabbitMessageQueuePoisonSetup(_channel, _queueName, useQueueNameAsRoutingKey: true);
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

            _channel.QueueDeclare(
                _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: _poisonSetup.GetRoutingArguments());

            _channel.BasicPublish(
                string.Empty,
                _queueName,
                default,
                messageBytes);
        }
        return Task.CompletedTask;
    }
}
