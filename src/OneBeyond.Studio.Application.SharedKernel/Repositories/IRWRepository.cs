using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Application.SharedKernel.Repositories.Exceptions;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Application.SharedKernel.Repositories;

/// <summary>
/// Read-write repository over an aggregate root keyed by <typeparamref name="TAggregateRootId"/>.
/// Returned aggregates are tracked, so changes to them are observed by the next write operation.
/// </summary>
/// <remarks>
/// Each write operation persists its change to the underlying store before the returned task
/// completes; there is no separate "save" step to call.
/// </remarks>
/// <typeparam name="TAggregateRoot">Type of the aggregate root.</typeparam>
/// <typeparam name="TAggregateRootId">Type of the aggregate root identifier.</typeparam>
public interface IRWRepository<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
    where TAggregateRootId : notnull
{
    /// <summary>
    /// Returns the tracked aggregate root identified by <paramref name="aggregateRootId"/>.
    /// </summary>
    /// <param name="aggregateRootId">Identifier of the aggregate root to load.</param>
    /// <param name="includes">Related object graph to load alongside the aggregate, or <see langword="null"/> for none.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The matching aggregate root; never <see langword="null"/>.</returns>
    /// <exception cref="EntityNotFoundException">No aggregate root with <paramref name="aggregateRootId"/> exists.</exception>
    /// <exception cref="EntityAccessDeniedException">The aggregate exists but the active read policy denies access to it.</exception>
    Task<TAggregateRoot> GetByIdAsync(
        TAggregateRootId aggregateRootId,
        Includes<TAggregateRoot>? includes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the single tracked aggregate root matching <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">Predicate that must identify at most one aggregate root.</param>
    /// <param name="includes">Related object graph to load alongside the aggregate, or <see langword="null"/> for none.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The matching aggregate root; never <see langword="null"/>.</returns>
    /// <exception cref="EntityNotFoundException">No aggregate root matches <paramref name="filter"/>.</exception>
    /// <exception cref="EntityAccessDeniedException">A match exists but the active read policy denies access to it.</exception>
    /// <exception cref="InvalidOperationException">More than one aggregate root matches <paramref name="filter"/>.</exception>
    Task<TAggregateRoot> GetByFilterAsync(
        Expression<Func<TAggregateRoot, bool>> filter,
        Includes<TAggregateRoot>? includes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Adds <paramref name="aggregateRoot"/> and persists it to the underlying store.
    /// </summary>
    /// <param name="aggregateRoot">Aggregate root to add.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="aggregateRoot"/> is <see langword="null"/>.</exception>
    /// <exception cref="EntityAccessDeniedException">The active create policy denies adding this aggregate.</exception>
    Task CreateAsync(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists changes to <paramref name="aggregateRoot"/> to the underlying store.
    /// </summary>
    /// <param name="aggregateRoot">Aggregate root whose changes are to be saved.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="aggregateRoot"/> is <see langword="null"/>.</exception>
    /// <exception cref="EntityAccessDeniedException">The active update policy denies updating this aggregate.</exception>
    Task UpdateAsync(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes <paramref name="aggregateRoot"/> and persists the deletion to the underlying store.
    /// </summary>
    /// <param name="aggregateRoot">Aggregate root to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="aggregateRoot"/> is <see langword="null"/>.</exception>
    /// <exception cref="EntityAccessDeniedException">The active delete policy denies deleting this aggregate.</exception>
    Task DeleteAsync(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the aggregate root identified by <paramref name="id"/> directly in the underlying store,
    /// or does nothing when no such aggregate exists.
    /// </summary>
    /// <remarks>
    /// This overload deletes by key without loading or tracking the aggregate. It therefore bypasses the
    /// delete access policy and the dispatch of any queued domain events. Use
    /// <see cref="DeleteAsync(TAggregateRoot, CancellationToken)"/> when either is required.
    /// </remarks>
    /// <param name="id">Identifier of the aggregate root to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task DeleteAsync(
        TAggregateRootId id,
        CancellationToken cancellationToken);
}
