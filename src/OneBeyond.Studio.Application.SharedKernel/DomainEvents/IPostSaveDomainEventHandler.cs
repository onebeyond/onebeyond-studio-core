using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.Application.SharedKernel.DomainEvents;

/// <summary>
/// Handles domain events of the <typeparamref name="TDomainEvent"/> type after an entity the event was raised on gets persisted.
/// Handler can be used for implementing side-effects outside of the transaction scope and their result will not affect already persisted entity.
/// For example, a notification can be send from such handler.
/// </summary>
/// <typeparam name="TDomainEvent"></typeparam>
public interface IPostSaveDomainEventHandler<in TDomainEvent>
    where TDomainEvent : DomainEvent
{
    Task HandleAsync(
        TDomainEvent domainEvent,
        IReadOnlyDictionary<string, object> domainEventAmbientContext,
        CancellationToken cancellationToken);
}
