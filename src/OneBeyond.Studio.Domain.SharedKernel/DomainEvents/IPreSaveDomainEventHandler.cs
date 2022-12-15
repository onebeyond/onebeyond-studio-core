using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Handles domain events of the <typeparamref name="TDomainEvent"/> type before an entity the event was raised on gets persisted.
/// Handler can be used for implementing side-effects in the same transaction where the entity gets persisted.
/// If the handler fails, the entire transaction will be rolled back.
/// </summary>
/// <typeparam name="TDomainEvent"></typeparam>
public interface IPreSaveDomainEventHandler<in TDomainEvent>
    where TDomainEvent : DomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
