using System.Diagnostics;
using EnsureThat;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Application.SharedKernel.DomainEvents;
using OneBeyond.Studio.Crosscuts.Activities;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Hosting.BackgroundServices;

namespace OneBeyond.Studio.Hosting.DomainEvents;

internal abstract class DomainEventRelayBase : BackgroundService, IBackgroundService
{
    private const string DomainEventRelayActivityName = "OneBeyond.Hosting.DomainEventRelay";
    private const string DomainEventInActivityName = "OneBeyond.Hosting.DomainEventIn";

    private static readonly ILogger Logger = LogManager.CreateLogger<DomainEventRelayBase>();

    private readonly IReadOnlyCollection<IRaisedDomainEventReceiver> _raisedDomainEventReceivers;

    protected DomainEventRelayBase(
        IReadOnlyCollection<IRaisedDomainEventReceiver> raisedDomainEventReceivers) // We allow to relay domain events from multiple receivers hosted in the same app
    {
        EnsureArg.HasItems(raisedDomainEventReceivers, nameof(raisedDomainEventReceivers));
        _raisedDomainEventReceivers = raisedDomainEventReceivers;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation(
            "Start relaying domain events from {RaisedDomainEventReceiverCount} domain event receiver(s)",
            _raisedDomainEventReceivers.Count);

        using (var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken))
        {
            var cancellationToken = cancellationTokenSource.Token;
            var relayTasks = _raisedDomainEventReceivers.Select(
                async (raisedDomainEventReceiver, idx) =>
                {
                    using (var activity = new Activity(DomainEventRelayActivityName).DoStart(
                        Activity.Current?.Id,
                        Activity.Current?.TraceStateString))
                    {
                        await raisedDomainEventReceiver.RunAsync(
                            async (raisedDomainEvent, cancellationToken) =>
                            {
                                using (var activity = new Activity(DomainEventInActivityName).DoStart(
                                    raisedDomainEvent.ActivityId,
                                    raisedDomainEvent.ActivityTraceState))
                                {
                                    await RelayAsync(raisedDomainEvent, cancellationToken);
                                }
                            },
                            cancellationToken);
                    }
                });
            await Task.WhenAny(relayTasks);
            cancellationTokenSource.Cancel();
        }
    }

    protected abstract Task RelayAsync(RaisedDomainEvent raisedDomainEvent, CancellationToken cancellationToken);

    Task IBackgroundService.ExecuteAsync(CancellationToken stoppingToken)
        => ExecuteAsync(stoppingToken);
}
