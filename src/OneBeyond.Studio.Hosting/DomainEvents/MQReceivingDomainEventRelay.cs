using System.Diagnostics;
using EnsureThat;
using Microsoft.Extensions.Hosting;
using OneBeyond.Studio.Crosscuts.Activities;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;
using OneBeyond.Studio.Hosting.BackgroundServices;

namespace OneBeyond.Studio.Hosting.DomainEvents
{
    internal sealed class MQReceivingDomainEventRelay<TDiscriminator> : BackgroundService, IBackgroundService
    {
        private const string DomainEventInActivityName = "OneBeyond.Hosting.DomainEventIn";

        private readonly IMessageQueueReceiver<RaisedDomainEvent> _domainEventQueueReceiver;
        private readonly IPostSaveDomainEventDispatcher _domainEventDispatcher;

        public MQReceivingDomainEventRelay(
            IMessageQueueReceiver<RaisedDomainEvent> domainEventQueueReceiver,
            IPostSaveDomainEventDispatcher domainEventDispatcher)
        {
            EnsureArg.IsNotNull(domainEventQueueReceiver, nameof(domainEventQueueReceiver));
            EnsureArg.IsNotNull(domainEventDispatcher, nameof(domainEventDispatcher));

            _domainEventQueueReceiver = domainEventQueueReceiver;
            _domainEventDispatcher = domainEventDispatcher;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _domainEventQueueReceiver.RunAsync(
                async (raisedDomainEvent, cancellationToken) =>
                {
                    using (var activity = new Activity(DomainEventInActivityName).DoStart(
                        raisedDomainEvent.ActivityId,
                        raisedDomainEvent.ActivityTraceState))
                    {
                        var domainEvent = raisedDomainEvent.GetValue();
                        var domainEventAmbientState = raisedDomainEvent.GetAmbientContext();

                        await _domainEventDispatcher.DispatchAsync(domainEvent, domainEventAmbientState, cancellationToken);
                    }
                },
                stoppingToken);
        }

        Task IBackgroundService.ExecuteAsync(CancellationToken stoppingToken)
            => ExecuteAsync(stoppingToken);
    }
}
