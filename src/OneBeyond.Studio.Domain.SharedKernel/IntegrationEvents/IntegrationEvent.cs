using System;
using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

/// <summary>
/// </summary>
public abstract class IntegrationEvent
{
    /// <summary>
    /// </summary>
    /// <param name="raisedAt"></param>
    protected IntegrationEvent(DateTimeOffset raisedAt)
    {
        EnsureArg.IsNotDefault(raisedAt, nameof(raisedAt));

        RaisedAt = raisedAt;
    }

    /// <summary>
    /// </summary>
    public DateTimeOffset RaisedAt { get; }
}
