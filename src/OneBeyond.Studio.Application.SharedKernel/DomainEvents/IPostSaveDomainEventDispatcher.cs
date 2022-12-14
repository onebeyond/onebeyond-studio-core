using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.Application.SharedKernel.DomainEvents;

public interface IPostSaveDomainEventDispatcher
{
    /// <summary>
    /// Dispatches domain events by calling
    /// <see cref="IPostSaveDomainEventHandler{TDomainEvent}.HandleAsync(TDomainEvent, IReadOnlyDictionary{string,object}, CancellationToken)" />
    /// after an entity the event was raised on gets persisted.
    /// </summary>
    Task DispatchAsync(
        DomainEvent domainEvent,
        IReadOnlyDictionary<string, object>? domainEventAmbientContext,
        CancellationToken cancellationToken);
}
