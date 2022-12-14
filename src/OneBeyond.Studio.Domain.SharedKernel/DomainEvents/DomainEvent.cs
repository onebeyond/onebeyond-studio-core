using System;
using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Base class for any domain event in the system.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// </summary>
    /// <param name="dateRaised"></param>
    protected DomainEvent(DateTimeOffset dateRaised)
    {
        EnsureArg.IsNotDefault(dateRaised, nameof(dateRaised));

        DateRaised = dateRaised;
    }

    /// <summary>
    /// Date and time the domain event raised.
    /// </summary>
    public DateTimeOffset DateRaised { get; private set; }
}
