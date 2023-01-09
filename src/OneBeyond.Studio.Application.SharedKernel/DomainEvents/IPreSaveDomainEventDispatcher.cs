using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.Application.SharedKernel.DomainEvents;

public interface IPreSaveDomainEventDispatcher
{
    /// <summary>
    /// Dispatches domain events by calling
    /// <see cref="IPreSaveDomainEventHandler{TDomainEvent}.HandleAsync(TDomainEvent, CancellationToken)"/>
    /// before an entity the event was raised on gets persisted.
    /// </summary>
    Task DispatchAsync(
        DomainEvent domainEvent,
        CancellationToken cancellationToken);
}
