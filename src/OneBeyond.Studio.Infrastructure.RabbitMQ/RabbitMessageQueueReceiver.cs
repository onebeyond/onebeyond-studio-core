using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.RabbitMQ.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OneBeyond.Studio.Infrastructure.RabbitMQ;

internal sealed class RabbitMessageQueueReceiver<TMessage>
    : IMessageQueueReceiver<TMessage>
    , IDisposable
{
    private static readonly ILogger Logger = LogManager.CreateLogger<RabbitMessageQueueReceiver<TMessage>>();

    private readonly string _queueName;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMessageQueueReceiver(RabbitMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));
        EnsureArg.IsNotNull(options.Connection, nameof(options.Connection));
        EnsureArg.IsNotNullOrWhiteSpace(options.QueueName, nameof(options.QueueName));

        var connectionFactory = new ConnectionFactory
        {
            Uri = options.Connection,
            ConsumerDispatchConcurrency = 4            
        };

        _queueName = options.QueueName!;
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        var poisonSetup = new RabbitMessageQueuePoisonSetup(_channel, _queueName, useQueueNameAsRoutingKey: true);

        _channel.QueueDeclare(
            _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: poisonSetup.GetRoutingArguments());

        _channel.BasicQos(0, 10, false); // TODO: Needs to be configurable. Align with Azure queue defaults.
    }    

    public void Dispose()
    {
        _channel.Close();
        _channel.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    public Task RunAsync(
        Func<TMessage, CancellationToken, Task> processAsync,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(processAsync, nameof(processAsync));

        return RunAsync(
            (messageBytes, cancellationToken) =>
            {
                var messageString = Encoding.UTF8.GetString(messageBytes);
                var message = JsonConvert.DeserializeObject<TMessage>(messageString)!;
                return processAsync(message, cancellationToken);
            },
            cancellationToken);
    }

    public async Task RunAsync(
        Func<byte[], CancellationToken, Task> processAsync,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(processAsync, nameof(processAsync));

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received +=
            async (sender, eventArgs) =>
            {
                var messageBytes = default(byte[]);
                try
                {
                    messageBytes = eventArgs.Body.ToArray();
                    await processAsync(messageBytes, cancellationToken);
                    ((AsyncEventingBasicConsumer)sender).Model.BasicAck(eventArgs.DeliveryTag, false);
                }
                catch (Exception exception)
                when (!exception.IsCritical())
                {
                    Logger.LogError(
                        exception,
                        "Failed to process message {QueueMessage} from queue {QueueName}. Message is nacked without re-queueing",
                        messageBytes is not null
                            ? Encoding.UTF8.GetString(messageBytes)
                            : null,
                        _queueName);
                    ((AsyncEventingBasicConsumer)sender).Model.BasicNack(eventArgs.DeliveryTag, false, false);
                }
            };

        var consumerTag = _channel.BasicConsume(
            _queueName,
            autoAck: false,
            consumer: consumer);

        try
        {
            await Task.Delay(-1, cancellationToken);
        }
        catch (TaskCanceledException taskCancelledException)
        when (!taskCancelledException.IsCritical())
        {
            _channel.BasicCancel(consumerTag);
        }
    }
}
