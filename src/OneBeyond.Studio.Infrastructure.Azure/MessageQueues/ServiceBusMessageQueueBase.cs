using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EnsureThat;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Crosscuts.Strings;
using OneBeyond.Studio.Infrastructure.Azure.Exceptions;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues;

internal abstract class ServiceBusMessageQueueBase<TMessage>
    : IMessageQueue<TMessage>
    , IAsyncDisposable
{
    protected ServiceBusMessageQueueBase(AzureServiceBusMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(options, nameof(options));
        options.EnsureIsValid();

        _ensureTopicExists = new AsyncLazy<bool>(
            async () =>
            {
                var requiresSession = !options.SessionId.IsNullOrWhiteSpace();
                var serviceBusAdminClient = string.IsNullOrWhiteSpace(options.ResourceName)
                    ? new ServiceBusAdministrationClient(options.ConnectionString)
                    : new ServiceBusAdministrationClient($"{options.ResourceName}.servicebus.windows.net", new DefaultAzureCredential());
                
                var queueExists = await serviceBusAdminClient.QueueExistsAsync(
                        options.QueueName)
                    .ConfigureAwait(false);
                if (queueExists)
                {
                    var queueProperties = await serviceBusAdminClient.GetQueueAsync(options.QueueName)
                        .ConfigureAwait(false);
                    if (queueProperties.Value.RequiresSession != requiresSession)
                    {
                        throw new AzureInfrastructureException(
                            $"ASB queue {options.QueueName} has configured {(queueProperties.Value.RequiresSession ? "to" : "not to")} " +
                            $"use sessions whereas requested behaviour is opposite.");
                    }
                }
                else
                {
                    await serviceBusAdminClient.CreateQueueAsync(
                            new CreateQueueOptions(options.QueueName)
                            {
                                RequiresSession = requiresSession
                            })
                        .ConfigureAwait(false);
                }
                return true;
            },
            AsyncLazyFlags.RetryOnFailure);
        
        _serviceBusClient = string.IsNullOrWhiteSpace(options.ResourceName)
                  ? new ServiceBusClient(options.ConnectionString)
                  : new ServiceBusClient($"{options.ResourceName}.servicebus.windows.net", new DefaultAzureCredential());
        _serviceBusSender = _serviceBusClient.CreateSender(options.QueueName);
        _sessionIdToken = options.SessionId;
    }

    private readonly AsyncLazy<bool> _ensureTopicExists;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSender _serviceBusSender;
    private readonly string? _sessionIdToken;

    public ValueTask DisposeAsync()
        => _serviceBusClient.DisposeAsync();

    public async Task PublishAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        EnsureArg.HasValue(message, nameof(message));

        await _ensureTopicExists.Task.ConfigureAwait(false);
        var messageJson = JObject.FromObject(message);
        var sessionId = _sessionIdToken.IsNullOrWhiteSpace()
            ? default
            : GetMessageSessionId(messageJson, _sessionIdToken);
        await _serviceBusSender.SendMessageAsync(new ServiceBusMessage(messageJson.ToString())
        {
            SessionId = sessionId
        },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static string GetMessageSessionId(JObject messageJson, string sessionIdToken)
    {
        var sessionId = messageJson.GetValue(
                sessionIdToken,
                StringComparison.InvariantCultureIgnoreCase)
            ?.ToString();
        return sessionId.IsNullOrWhiteSpace()
            ? throw new AzureInfrastructureException(
                $"Unable to get message session ID from {sessionIdToken}.")
            : sessionId;
    }
}
