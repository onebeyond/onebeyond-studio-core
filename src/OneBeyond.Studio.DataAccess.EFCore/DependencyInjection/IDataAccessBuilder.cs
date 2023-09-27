using System;
using System.Transactions;
using OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

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
    /// <returns></returns>
    IDataAccessBuilder WithUnitOfWork(TimeSpan? timeout = default, IsolationLevel? isolationLevel = default);

    /// <summary>
    /// Enables domain events.
    /// </summary>
    /// <returns></returns>
    IDataAccessBuilder WithDomainEvents();

    /// <summary>
    /// Enables domain and integration events.
    /// </summary>
    /// <returns></returns>
    IDataAccessBuilder WithDomainAndIntegrationEvents<TIntegrationEventDispatcher>()
        where TIntegrationEventDispatcher : class, IIntegrationEventDispatcher;

    /// <summary>
    /// Enables domain and integration events (using default DI dispatcher).
    /// </summary>
    /// <returns></returns>
    IDataAccessBuilder WithDomainAndIntegrationEvents();
}
