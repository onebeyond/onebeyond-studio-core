using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities;

/// <summary>
/// Base class for entity with domain event handling.
/// </summary>
public abstract class DomainEntity
{
    private readonly List<DomainEvent> _domainEvents;

    /// <summary>
    /// </summary>
    protected DomainEntity()
        => _domainEvents = new List<DomainEvent>();

    /// <summary>
    /// Entity's ID as string.
    /// </summary>
    public abstract string IdAsString { get; }

    /// <summary>
    /// Raises a domain event with regard to this entity. The time the event gets dispatched depends on domain event dispatcher implementation.
    /// </summary>
    /// <param name="domainEvent"></param>
    public void RaiseDomainEvent(DomainEvent domainEvent)
    {
        EnsureArg.IsNotNull(domainEvent, nameof(domainEvent));
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Lists domain events raised so far. The method is supposed to be used by domain event processors only.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<DomainEvent> ListDomainEvents()
        => _domainEvents.AsReadOnly();

    /// <summary>
    /// Releases domain events raised so far. The method is supposed to be used by domain event processors only.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> ReleaseDomainEvents()
    {
        var domainEvents = _domainEvents.ToList();
        _domainEvents.Clear();
        return domainEvents;
    }
}

/// <summary>
/// Base class for entity with an ID of the <typeparamref name="TEntityId"/> type.
/// </summary>
/// <typeparam name="TEntityId"></typeparam>
public abstract class DomainEntity<TEntityId> : DomainEntity
{
    /// <summary>
    /// </summary>
    /// <param name="entityId"></param>
    protected DomainEntity(TEntityId entityId)
    {
        EnsureArg.IsFalse(
            EqualityComparer<TEntityId>.Default.Equals(entityId, default!),
            nameof(entityId));

        Id = entityId;
    }

    /// <summary>
    /// It is mostly for persistance only.
    /// </summary>
    protected DomainEntity()
    {
        Id = default!;
    }

    /// <summary>
    /// Entity's ID
    /// </summary>
    public TEntityId Id { get; private set; }

    /// <summary>
    /// Entity's ID as string
    /// </summary>
    public override string IdAsString => Id!.ToString()!;
}
