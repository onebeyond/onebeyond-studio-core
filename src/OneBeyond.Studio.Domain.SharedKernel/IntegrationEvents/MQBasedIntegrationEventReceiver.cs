using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Crosscuts.MessageQueues;

namespace OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

internal sealed class MQBasedIntegrationEventReceiver : IIntegrationEventReceiver
{
    private static readonly ILogger Logger = LogManager.CreateLogger<MQBasedIntegrationEventReceiver>();

    private readonly IMessageQueueReceiver<IntegrationEventEnvelope> _integrationEventQueue;
    private readonly IntegrationEventTypeRegistry _integrationEventTypeRegistry;

    public MQBasedIntegrationEventReceiver(
        IMessageQueueReceiver<IntegrationEventEnvelope> integrationEventQueue,
        IReadOnlyCollection<Assembly> integrationEventAssemblies)
    {
        EnsureArg.IsNotNull(integrationEventQueue, nameof(integrationEventQueue));
        EnsureArg.IsNotNull(integrationEventAssemblies, nameof(integrationEventAssemblies));

        _integrationEventQueue = integrationEventQueue;
        _integrationEventTypeRegistry = new IntegrationEventTypeRegistry(integrationEventAssemblies);
    }

    public Task RunAsync(
        Func<IntegrationEvent, CancellationToken, Task> processAsync,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(processAsync, nameof(processAsync));

        return _integrationEventQueue.RunAsync(
            (integrationEventEnvelope, cancellationToken) =>
            {
                var (integrationEventMajorVersion, integrationEventMinorVersion) =
                    IntegrationEventVersion.FromString(integrationEventEnvelope.Version);

                var integrationEventType = _integrationEventTypeRegistry.FindIntegrationEventType(
                    integrationEventEnvelope.TypeName,
                    integrationEventMajorVersion,
                    integrationEventMinorVersion);

                if (integrationEventType is null)
                {
                    Logger.LogInformation(
                        "Skip receiving unrecognized integration event of {IntegrationEventTypeName} type and version {IntegrationEventVersion}",
                        integrationEventEnvelope.TypeName,
                        integrationEventEnvelope.Version);
                    return Task.CompletedTask;
                }

                Logger.LogInformation(
                    "Deserializing integration event of {IntegrationEventTypeName} type and version {IntegrationEventVersion}"
                    + " via {IntegrationEventClrType} CLR type and version {IntegrationEventVersion1}",
                    integrationEventEnvelope.TypeName,
                    integrationEventEnvelope.Version,
                    integrationEventType.ClrType.AssemblyQualifiedName,
                    integrationEventType.Version);

                var integrationEvent = (IntegrationEvent)integrationEventEnvelope.Data.ToObject(integrationEventType.ClrType)!;

                return processAsync(integrationEvent, cancellationToken);
            },
            cancellationToken);
    }
}
