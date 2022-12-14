using EnsureThat;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.Hosting.DomainEvents
{
    internal sealed class MQPublishingDomainEventRelay<TDiscriminator> : DomainEventRelayBase
    {
        private readonly IMessageQueue<RaisedDomainEvent> _domainEventQueue;

        public MQPublishingDomainEventRelay(
            IReadOnlyCollection<IRaisedDomainEventReceiver> raisedDomainEventReceivers,
            IMessageQueue<RaisedDomainEvent> domainEventQueue)
            : base(raisedDomainEventReceivers)
        {
            EnsureArg.IsNotNull(domainEventQueue, nameof(domainEventQueue));

            _domainEventQueue = domainEventQueue;
        }

        protected override Task RelayAsync(RaisedDomainEvent raisedDomainEvent, CancellationToken cancellationToken)
            => _domainEventQueue.PublishAsync(raisedDomainEvent, cancellationToken);
    }
}
