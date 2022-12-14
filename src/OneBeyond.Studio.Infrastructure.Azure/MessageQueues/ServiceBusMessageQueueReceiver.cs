using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nito.AsyncEx;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues;

internal sealed class ServiceBusMessageQueueReceiver<TMessage>
    : IMessageQueueReceiver<TMessage>
    , IAsyncDisposable
{
    private static readonly ILogger Logger = LogManager.CreateLogger<ServiceBusMessageQueueReceiver<TMessage>>();

    private readonly ServiceBusClient _client;
    private readonly ServiceBusReceiver _messageReceiver;

    public ServiceBusMessageQueueReceiver(AzureMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));
        options.EnsureIsValid();

        _ensureQueueExists = new AsyncLazy<bool>(
            async () =>
            {
                var serviceBusAdminClient = new ServiceBusAdministrationClient(options.ConnectionString);
                var queueExists = await serviceBusAdminClient.QueueExistsAsync(
                        options.QueueName)
                    .ConfigureAwait(false);

                if (!queueExists)
                {
                    await serviceBusAdminClient.CreateQueueAsync(
                          new CreateQueueOptions(options.QueueName))
                      .ConfigureAwait(false);
                }

                return true;
            },
            AsyncLazyFlags.RetryOnFailure);

        var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
        _client = new ServiceBusClient(options.ConnectionString, clientOptions);
        _messageReceiver = _client.CreateReceiver(options.QueueName);
    }

    private readonly AsyncLazy<bool> _ensureQueueExists;

    public async ValueTask DisposeAsync()
    {
        await _messageReceiver.CloseAsync()
            .ConfigureAwait(false);
        await _messageReceiver.DisposeAsync()
            .ConfigureAwait(false);
        await _client.DisposeAsync()
            .ConfigureAwait(false);
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

    public async Task RunAsync(Func<byte[], CancellationToken, Task> processAsync, CancellationToken cancellationToken)
    {
        await _ensureQueueExists.Task;

        while (!cancellationToken.IsCancellationRequested && !_messageReceiver.IsClosed)
        {
            var message = await _messageReceiver.ReceiveMessageAsync(
                    TimeSpan.MaxValue,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            try
            {
                await processAsync(message.Body.ToArray(), cancellationToken)
                .ConfigureAwait(false);

                await _messageReceiver.CompleteMessageAsync(message, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                //The message would be put in the dead letter automatically after 10 attempts.  
                Logger.LogError(exception, "Exception occured when processing a message: {SequenceNumber}, " +
                        "with body: {Body}. attempt: {DeliveryCount}", message.SequenceNumber, message.Body, message.DeliveryCount);

                await _messageReceiver.AbandonMessageAsync(message, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
