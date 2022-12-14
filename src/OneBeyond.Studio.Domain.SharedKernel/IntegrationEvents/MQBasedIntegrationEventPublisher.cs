using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Newtonsoft.Json.Linq;
using OneBeyond.Studio.Crosscuts.MessageQueues;

namespace OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

internal sealed class MQBasedIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IMessageQueue<IntegrationEventEnvelope> _integrationEventQueue;

    public MQBasedIntegrationEventPublisher(IMessageQueue<IntegrationEventEnvelope> integrationEventQueue)
    {
        EnsureArg.IsNotNull(integrationEventQueue, nameof(integrationEventQueue));

        _integrationEventQueue = integrationEventQueue;
    }

    public Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(integrationEvent, nameof(integrationEvent));

        var integrationEventClrType = integrationEvent.GetType();

        var integrationEventTypeName =
            IntegrationEventTypeAttribute.GetName(integrationEventClrType);

        var (integrationEventMajorVersion, integrationEventMinorVersion) =
            IntegrationEventVersionAttribute.GetVersion(integrationEventClrType);

        var integrationEventEnvelope = new IntegrationEventEnvelope(
            integrationEventTypeName,
            IntegrationEventVersion.ToString(integrationEventMajorVersion, integrationEventMinorVersion),
            JObject.FromObject(integrationEvent));

        return _integrationEventQueue.PublishAsync(integrationEventEnvelope, cancellationToken);
    }
}
