using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.Repositories.Exceptions;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Application.SharedKernel.Repositories;

/// <summary>
/// Convenience overloads for <see cref="IRWRepository{TAggregateRoot, TAggregateRootId}"/>.
/// </summary>
public static class IRWRepositoryExtensions
{
    /// <summary>
    /// Returns the tracked aggregate root identified by <paramref name="aggregateRootId"/>.
    /// </summary>
    /// <typeparam name="TAggregateRoot">Type of the aggregate root.</typeparam>
    /// <typeparam name="TAggregateRootId">Type of the aggregate root identifier.</typeparam>
    /// <param name="rwRepository">Repository to query.</param>
    /// <param name="aggregateRootId">Identifier of the aggregate root to load.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The matching aggregate root; never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rwRepository"/> is <see langword="null"/>.</exception>
    /// <exception cref="EntityNotFoundException">No aggregate root with <paramref name="aggregateRootId"/> exists.</exception>
    /// <exception cref="EntityAccessDeniedException">The aggregate exists but the active read policy denies access to it.</exception>
    public static Task<TAggregateRoot> GetByIdAsync<TAggregateRoot, TAggregateRootId>(
        this IRWRepository<TAggregateRoot, TAggregateRootId> rwRepository,
        TAggregateRootId aggregateRootId,
        CancellationToken cancellationToken)
        where TAggregateRoot : AggregateRoot<TAggregateRootId>
        where TAggregateRootId : notnull
    {
        EnsureArg.IsNotNull(rwRepository, nameof(rwRepository));

        return rwRepository.GetByIdAsync(
            aggregateRootId,
            default,
            cancellationToken);
    }

    /// <summary>
    /// Returns the single tracked aggregate root matching <paramref name="filter"/>.
    /// </summary>
    /// <typeparam name="TAggregateRoot">Type of the aggregate root.</typeparam>
    /// <typeparam name="TAggregateRootId">Type of the aggregate root identifier.</typeparam>
    /// <param name="rwRepository">Repository to query.</param>
    /// <param name="filter">Predicate that must identify at most one aggregate root.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The matching aggregate root; never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rwRepository"/> is <see langword="null"/>.</exception>
    /// <exception cref="EntityNotFoundException">No aggregate root matches <paramref name="filter"/>.</exception>
    /// <exception cref="EntityAccessDeniedException">A match exists but the active read policy denies access to it.</exception>
    /// <exception cref="InvalidOperationException">More than one aggregate root matches <paramref name="filter"/>.</exception>
    public static Task<TAggregateRoot> GetByFilterAsync<TAggregateRoot, TAggregateRootId>(
        this IRWRepository<TAggregateRoot, TAggregateRootId> rwRepository,
        Expression<Func<TAggregateRoot, bool>> filter,
        CancellationToken cancellationToken)
        where TAggregateRoot : AggregateRoot<TAggregateRootId>
        where TAggregateRootId : notnull
    {
        EnsureArg.IsNotNull(rwRepository, nameof(rwRepository));

        return rwRepository.GetByFilterAsync(
            filter,
            default,
            cancellationToken);
    }
}
