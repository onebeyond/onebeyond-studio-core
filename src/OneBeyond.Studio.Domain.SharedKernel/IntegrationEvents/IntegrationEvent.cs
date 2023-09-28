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
    }

    protected IntegrationEvent(DateTimeOffset raisedAt)
        : base(raisedAt)
    {
    }
}
