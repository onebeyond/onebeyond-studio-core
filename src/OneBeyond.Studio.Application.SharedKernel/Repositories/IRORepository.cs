using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Application.SharedKernel.Repositories.Exceptions;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.Application.SharedKernel.Repositories;

/// <summary>
/// Read-only repository exposing query operations over <typeparamref name="TEntity"/>.
/// Results are detached.
/// </summary>
/// <typeparam name="TEntity">Type of the queried entity.</typeparam>
public interface IRORepository<TEntity>
{
    /// <summary>
    /// Returns the entities matching <paramref name="filter"/>, or all entities when it is <see langword="null"/>.
    /// </summary>
    /// <param name="filter">Predicate to match, or <see langword="null"/> to match every entity.</param>
    /// <param name="includes">Related object graph to load alongside each entity, or <see langword="null"/> for none.</param>
    /// <param name="paging">Slice of the result set to return, or <see langword="null"/> for all matches.</param>
    /// <param name="sortings">Ordering to apply.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<IReadOnlyCollection<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? filter = default,
        Includes<TEntity>? includes = default,
        Paging? paging = default,
        IReadOnlyCollection<Sorting<TEntity>>? sortings = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Projects the entities matching <paramref name="preFilter"/> onto <typeparamref name="TResultDto"/>,
    /// then returns those matching <paramref name="filter"/>.
    /// </summary>
    /// <remarks>
    /// <paramref name="preFilter"/> is applied in entity space before projection; <paramref name="filter"/>
    /// is applied in result space after it. The projection is resolved from the registered mapping for
    /// <typeparamref name="TResultDto"/>.
    /// </remarks>
    /// <typeparam name="TResultDto">Type each entity is projected onto.</typeparam>
    /// <param name="preFilter">Predicate applied to entities before projection, or <see langword="null"/> to match all.</param>
    /// <param name="filter">Predicate applied to projected results, or <see langword="null"/> to match all.</param>
    /// <param name="paging">Slice of the result set to return, or <see langword="null"/> for all matches.</param>
    /// <param name="sortings">Ordering to apply.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<IReadOnlyCollection<TResultDto>> ListAsync<TResultDto>(
        Expression<Func<TEntity, bool>>? preFilter,
        Expression<Func<TResultDto, bool>>? filter = default,
        Paging? paging = default,
        IReadOnlyCollection<Sorting<TResultDto>>? sortings = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the entities matching <paramref name="filter"/> projected onto <typeparamref name="TResultDto"/>
    /// via <paramref name="projection"/>.
    /// </summary>
    /// <typeparam name="TResultDto">Type each entity is projected onto.</typeparam>
    /// <param name="projection">Expression mapping an entity to a result. Translated to the query, not run in memory.</param>
    /// <param name="filter">Predicate applied to entities before projection, or <see langword="null"/> to match all.</param>
    /// <param name="paging">Slice of the result set to return, or <see langword="null"/> for all matches.</param>
    /// <param name="sortings">Ordering applied to entities before projection.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<IReadOnlyCollection<TResultDto>> ListAsync<TResultDto>(
        Expression<Func<TEntity, TResultDto>> projection,
        Expression<Func<TEntity, bool>>? filter = default,
        Paging? paging = default,
        IReadOnlyCollection<Sorting<TEntity>>? sortings = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the entities matching <paramref name="filter"/>, or all entities when it is <see langword="null"/>.
    /// </summary>
    /// <param name="filter">Predicate to match, or <see langword="null"/> to count every entity.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? filter = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the entities matching <paramref name="preFilter"/> whose <typeparamref name="TResultDto"/>
    /// projection matches <paramref name="filter"/>.
    /// </summary>
    /// <typeparam name="TResultDto">Type each entity is projected onto.</typeparam>
    /// <param name="preFilter">Predicate applied to entities before projection, or <see langword="null"/> to match all.</param>
    /// <param name="filter">Predicate applied to projected results, or <see langword="null"/> to match all.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<int> CountAsync<TResultDto>(
        Expression<Func<TEntity, bool>>? preFilter,
        Expression<Func<TResultDto, bool>>? filter = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any entity matches <paramref name="filter"/>, or whether any entity exists
    /// when it is <see langword="null"/>.
    /// </summary>
    /// <param name="filter">Predicate to match, or <see langword="null"/> to match every entity.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? filter = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any entity matching <paramref name="preFilter"/> has a <typeparamref name="TResultDto"/>
    /// projection matching <paramref name="filter"/>.
    /// </summary>
    /// <typeparam name="TResultDto">Type each entity is projected onto.</typeparam>
    /// <param name="preFilter">Predicate applied to entities before projection, or <see langword="null"/> to match all.</param>
    /// <param name="filter">Predicate applied to projected results, or <see langword="null"/> to match all.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<bool> AnyAsync<TResultDto>(
        Expression<Func<TEntity, bool>>? preFilter,
        Expression<Func<TResultDto, bool>>? filter = default,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Read-only repository that adds identifier-based lookups for an entity keyed by
/// <typeparamref name="TEntityId"/>.
/// </summary>
/// <typeparam name="TEntity">Type of the queried entity.</typeparam>
/// <typeparam name="TEntityId">Type of the entity identifier.</typeparam>
public interface IRORepository<TEntity, TEntityId> : IRORepository<TEntity>
    where TEntity : DomainEntity<TEntityId>
    where TEntityId : notnull
{
    /// <summary>
    /// Returns the entity identified by <paramref name="entityId"/>.
    /// </summary>
    /// <param name="entityId">Identifier of the entity to load.</param>
    /// <param name="includes">Related object graph to load alongside the entity, or <see langword="null"/> for none.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The matching entity; never <see langword="null"/>.</returns>
    /// <exception cref="EntityNotFoundException">No entity with <paramref name="entityId"/> exists.</exception>
    /// <exception cref="EntityAccessDeniedException">The entity exists but the active read policy denies access to it.</exception>
    Task<TEntity> GetByIdAsync(
        TEntityId entityId,
        Includes<TEntity>? includes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the entity identified by <paramref name="entityId"/> projected onto <typeparamref name="TResultDto"/>
    /// using the registered mapping.
    /// </summary>
    /// <typeparam name="TResultDto">Type the entity is projected onto.</typeparam>
    /// <param name="entityId">Identifier of the entity to load.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The projected result; never <see langword="null"/>.</returns>
    /// <exception cref="EntityNotFoundException">No entity with <paramref name="entityId"/> exists.</exception>
    /// <exception cref="EntityAccessDeniedException">The entity exists but the active read policy denies access to it.</exception>
    Task<TResultDto> GetByIdAsync<TResultDto>(
        TEntityId entityId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the entity identified by <paramref name="entityId"/> projected onto <typeparamref name="TResultDto"/>
    /// via <paramref name="projection"/>.
    /// </summary>
    /// <typeparam name="TResultDto">Type the entity is projected onto.</typeparam>
    /// <param name="entityId">Identifier of the entity to load.</param>
    /// <param name="projection">Expression mapping the entity to a result. Translated to the query, not run in memory.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The projected result; never <see langword="null"/>.</returns>
    /// <exception cref="EntityNotFoundException">No entity with <paramref name="entityId"/> exists.</exception>
    /// <exception cref="EntityAccessDeniedException">The entity exists but the active read policy denies access to it.</exception>
    Task<TResultDto> GetByIdAsync<TResultDto>(
        TEntityId entityId,
        Expression<Func<TEntity, TResultDto>> projection,
        CancellationToken cancellationToken);
}
