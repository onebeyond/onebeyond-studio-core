using System;
using System.Transactions;

namespace OneBeyond.Studio.DataAccess.EFCore.DependencyInjection;

/// <summary>
/// Configures infrastructure.
/// </summary>
public interface IDataAccessBuilder
{
    /// <summary>
    /// Enables unit of work.
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="isolationLevel"></param>
    IDataAccessBuilder WithUnitOfWork(TimeSpan? timeout = default, IsolationLevel? isolationLevel = default);

    /// <summary>
    /// Enables domain events.
    /// </summary>
    IDataAccessBuilder WithDomainEvents();

    /// <summary>
    /// Enables domain entities change tracking.
    /// </summary>
    IDataAccessBuilder WithChangeTracking();
}
