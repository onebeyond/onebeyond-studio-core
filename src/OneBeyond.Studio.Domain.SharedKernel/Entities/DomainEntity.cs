using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities;

/// <summary>
/// Base class for entity with domain event and integration event handling.
/// </summary>
public abstract class DomainEntity
{
    private readonly List<DomainEvent> _domainEvents;
    private readonly List<IntegrationEvent> _integrationEvents;

    /// <summary>
    /// </summary>
    protected DomainEntity()
    {
        _domainEvents = new List<DomainEvent>();
        _integrationEvents = new List<IntegrationEvent>();
    }

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

    /// <summary>
    /// Raises a domain event with regard to this entity. The time the event gets dispatched depends on integration event dispatcher implementation.
    /// </summary>
    /// <param name="integrationEvent"></param>
    public void RaiseIntegrationEvent(IntegrationEvent integrationEvent)
    {
        EnsureArg.IsNotNull(integrationEvent, nameof(integrationEvent));
        _integrationEvents.Add(integrationEvent);

        if (integrationEvent.IsAlsoDomainEvent)
        {
            _domainEvents.Add(integrationEvent);
        }
    }

    /// <summary>
    /// Lists integration events raised so far. The method is supposed to be used by integration event processors only.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<IntegrationEvent> ListIntegrationEvents()
        => _integrationEvents.AsReadOnly();

    /// <summary>
    /// Releases integration events raised so far. The method is supposed to be used by integration event processors only.
    /// </summary>
    public IReadOnlyCollection<IntegrationEvent> ReleaseIntegrationEvents()
    {
        var integrationEvents = _integrationEvents.ToList();
        _integrationEvents.Clear();
        return integrationEvents;
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
