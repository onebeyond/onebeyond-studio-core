using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EnsureThat;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues;

internal abstract class ServiceBusPubSubMessageQueueBase<TMessage>
    : IMessageQueue<TMessage>
    , IAsyncDisposable
{

    protected ServiceBusPubSubMessageQueueBase(AzureServicePubSubBusMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));

        options.EnsureIsValid();

        _ensureTopicExists = new AsyncLazy<bool>(
            async () =>
            {
                var serviceBusAdminClient = new ServiceBusAdministrationClient(options.ConnectionString);
                var queueExists = await serviceBusAdminClient.TopicExistsAsync(
                        options.TopicName)
                    .ConfigureAwait(false);

                if (!queueExists)
                {
                    await serviceBusAdminClient.CreateTopicAsync(
                          new CreateTopicOptions(options.TopicName))
                      .ConfigureAwait(false);
                }

                return true;
            },
            AsyncLazyFlags.RetryOnFailure);

        _serviceBusClient = new ServiceBusClient(options.ConnectionString);
        _serviceBusSender = _serviceBusClient.CreateSender(options.TopicName);
    }

    private readonly AsyncLazy<bool> _ensureTopicExists;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSender _serviceBusSender;

    public ValueTask DisposeAsync()
        => _serviceBusClient.DisposeAsync();

    public async Task PublishAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        EnsureArg.HasValue(message, nameof(message));

        await _ensureTopicExists.Task.ConfigureAwait(false);
        var messageJson = JObject.FromObject(message);
        await _serviceBusSender.SendMessageAsync(new ServiceBusMessage(messageJson.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
