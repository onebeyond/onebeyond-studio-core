using EnsureThat;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Application.SharedKernel.DomainEvents;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Crosscuts.Logging;

namespace OneBeyond.Studio.Hosting.DomainEvents;

internal sealed class DomainEventRelay : DomainEventRelayBase
{
    private static readonly ILogger Logger = LogManager.CreateLogger<DomainEventRelay>();

    private readonly IPostSaveDomainEventDispatcher _domainEventDispatcher;

    public DomainEventRelay(
        IReadOnlyCollection<IRaisedDomainEventReceiver> raisedDomainEventReceivers,
        IPostSaveDomainEventDispatcher domainEventDispatcher)
        : base(raisedDomainEventReceivers)
    {
        EnsureArg.IsNotNull(domainEventDispatcher, nameof(domainEventDispatcher));

        _domainEventDispatcher = domainEventDispatcher;
    }

    protected override async Task RelayAsync(
        RaisedDomainEvent raisedDomainEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            var domainEvent = raisedDomainEvent.GetValue();
            var domainEventAmbientState = raisedDomainEvent.GetAmbientContext();

            Logger.LogInformation(
                "Domain event {@DomainEvent} of {DomainEventType} type with ambient state " +
                "{@DomainEventAmbientState} received from queue. Dispatching it...",
                domainEvent,
                domainEvent.GetType().AssemblyQualifiedName,
                domainEventAmbientState);

            await _domainEventDispatcher.DispatchAsync(domainEvent, domainEventAmbientState, cancellationToken);
        }
        catch (Exception exception)
        when (!exception.IsCritical())
        {
            Logger.LogCritical(
                exception,
                "Execution of domain event relay has terminated due to error");
            throw;
        }
    }
}
