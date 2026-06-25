using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.Repositories.Exceptions;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.Application.SharedKernel.Repositories;

/// <summary>
/// Convenience overloads for <see cref="IRORepository{TEntity, TEntityId}"/>.
/// </summary>
public static class IRORepositoryExtensions
{
    /// <summary>
    /// Returns the entity identified by <paramref name="entityId"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of the queried entity.</typeparam>
    /// <typeparam name="TEntityId">Type of the entity identifier.</typeparam>
    /// <param name="roRepository">Repository to query.</param>
    /// <param name="entityId">Identifier of the entity to load.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The matching entity; never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="roRepository"/> is <see langword="null"/>.</exception>
    /// <exception cref="EntityNotFoundException">No entity with <paramref name="entityId"/> exists.</exception>
    /// <exception cref="EntityAccessDeniedException">The entity exists but the active read policy denies access to it.</exception>
    public static Task<TEntity> GetByIdAsync<TEntity, TEntityId>(
        this IRORepository<TEntity, TEntityId> roRepository,
        TEntityId entityId,
        CancellationToken cancellationToken)
        where TEntity : DomainEntity<TEntityId>
        where TEntityId : notnull
    {
        EnsureArg.IsNotNull(roRepository, nameof(roRepository));

        return roRepository.GetByIdAsync(
            entityId,
            default,
            cancellationToken);
    }

    /// <summary>
    /// Projects every entity onto <typeparamref name="TResultDto"/> and returns those matching
    /// <paramref name="filter"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of the queried entity.</typeparam>
    /// <typeparam name="TEntityId">Type of the entity identifier.</typeparam>
    /// <typeparam name="TResultDto">Type each entity is projected onto.</typeparam>
    /// <param name="roRepository">Repository to query.</param>
    /// <param name="filter">Predicate applied to projected results, or <see langword="null"/> to match all.</param>
    /// <param name="paging">Slice of the result set to return, or <see langword="null"/> for all matches.</param>
    /// <param name="sortings">Ordering to apply.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="roRepository"/> is <see langword="null"/>.</exception>
    public static Task<IReadOnlyCollection<TResultDto>> ListAsync<TEntity, TEntityId, TResultDto>(
        this IRORepository<TEntity, TEntityId> roRepository,
        Expression<Func<TResultDto, bool>>? filter = default,
        Paging? paging = default,
        IReadOnlyCollection<Sorting<TResultDto>>? sortings = default,
        CancellationToken cancellationToken = default)
        where TEntity : DomainEntity<TEntityId>
        where TEntityId : notnull
    {
        EnsureArg.IsNotNull(roRepository, nameof(roRepository));

        return roRepository.ListAsync(
            default,
            filter,
            paging,
            sortings,
            cancellationToken);
    }

    /// <summary>
    /// Counts the entities whose <typeparamref name="TResultDto"/> projection matches <paramref name="filter"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of the queried entity.</typeparam>
    /// <typeparam name="TEntityId">Type of the entity identifier.</typeparam>
    /// <typeparam name="TResultDto">Type each entity is projected onto.</typeparam>
    /// <param name="roRepository">Repository to query.</param>
    /// <param name="filter">Predicate applied to projected results, or <see langword="null"/> to count all.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="roRepository"/> is <see langword="null"/>.</exception>
    public static Task<int> CountAsync<TEntity, TEntityId, TResultDto>(
        this IRORepository<TEntity, TEntityId> roRepository,
        Expression<Func<TResultDto, bool>>? filter = default,
        CancellationToken cancellationToken = default)
        where TEntity : DomainEntity<TEntityId>
        where TEntityId : notnull
    {
        EnsureArg.IsNotNull(roRepository, nameof(roRepository));

        return roRepository.CountAsync(
            default,
            filter,
            cancellationToken);
    }

    /// <summary>
    /// Determines whether any entity has a <typeparamref name="TResultDto"/> projection matching
    /// <paramref name="filter"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of the queried entity.</typeparam>
    /// <typeparam name="TEntityId">Type of the entity identifier.</typeparam>
    /// <typeparam name="TResultDto">Type each entity is projected onto.</typeparam>
    /// <param name="roRepository">Repository to query.</param>
    /// <param name="filter">Predicate applied to projected results, or <see langword="null"/> to match all.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="roRepository"/> is <see langword="null"/>.</exception>
    public static Task<bool> AnyAsync<TEntity, TEntityId, TResultDto>(
        this IRORepository<TEntity, TEntityId> roRepository,
        Expression<Func<TResultDto, bool>>? filter = default,
        CancellationToken cancellationToken = default)
        where TEntity : DomainEntity<TEntityId>
        where TEntityId : notnull
    {
        EnsureArg.IsNotNull(roRepository, nameof(roRepository));

        return roRepository.AnyAsync(
            default,
            filter,
            cancellationToken);
    }
}
