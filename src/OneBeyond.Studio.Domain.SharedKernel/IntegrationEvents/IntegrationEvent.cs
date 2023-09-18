using System;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

/// <summary>
/// </summary>
public abstract class IntegrationEvent : DomainEvent
{
    protected IntegrationEvent()
        : base(DateTime.UtcNow)
    {
        IsAlsoDomainEvent = true;
    }

    protected IntegrationEvent(bool isAlsoDomainEvent)
        : base(DateTime.UtcNow)
    {
        IsAlsoDomainEvent = isAlsoDomainEvent;
    }

    protected IntegrationEvent(DateTimeOffset raisedAt)
        : base(raisedAt)
    {
        IsAlsoDomainEvent = true;
    }

    protected IntegrationEvent(DateTimeOffset raisedAt, bool isAlsoDomainEvent)
        : base(raisedAt)
    {
        IsAlsoDomainEvent = isAlsoDomainEvent;
    }

    public bool IsAlsoDomainEvent { get; }
}
