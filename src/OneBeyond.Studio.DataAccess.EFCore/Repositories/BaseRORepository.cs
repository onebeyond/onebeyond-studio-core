using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DelegateDecompiler.EntityFrameworkCore;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using OneBeyond.Studio.DataAccess.EFCore.Projections;
using OneBeyond.Studio.Domain.SharedKernel.DataAccessPolicies;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Repositories;
using OneBeyond.Studio.Domain.SharedKernel.Repositories.Exceptions;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.DataAccess.EFCore.Repositories;

/// <summary>
/// Implements base read-only repository for the <typeparamref name="TEntity"/> type using EF Core.
/// All the entities returned by this repository are non-trackable.
/// </summary>
/// <typeparam name="TDbContext">DB context type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public class BaseRORepository<TDbContext, TEntity> : IRORepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    public BaseRORepository(
        TDbContext dbContext,
        IRODataAccessPolicyProvider<TEntity> roDataAccessPolicyProvider,
        IEntityTypeProjections<TEntity> entityTypeProjections)
    {
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));
        EnsureArg.IsNotNull(roDataAccessPolicyProvider, nameof(roDataAccessPolicyProvider));
        EnsureArg.IsNotNull(entityTypeProjections, nameof(entityTypeProjections));

        DbContext = dbContext;
        ReadDataAccessPolicy = new AsyncLazy<DataAccessPolicy<TEntity>?>(
            () => roDataAccessPolicyProvider.GetReadDataAccessPolicyAsync(),
            AsyncLazyFlags.RetryOnFailure);
        DbSet = new Lazy<DbSet<TEntity>>(() => DbContext.Set<TEntity>());
        EntityTypeProjections = entityTypeProjections;
    }

    protected TDbContext DbContext { get; }
    protected Lazy<DbSet<TEntity>> DbSet { get; }
    protected AsyncLazy<DataAccessPolicy<TEntity>?> ReadDataAccessPolicy { get; }
    protected IEntityTypeProjections<TEntity> EntityTypeProjections { get; }

    public async Task<IReadOnlyCollection<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Includes<TEntity>? includes = null,
        Paging? paging = null,
        IReadOnlyCollection<Sorting<TEntity>>? sortings = null,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildListQueryAsync(default, includes).ConfigureAwait(false);
        query = CompleteListQuery(query, filter, sortings, paging);
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<TResultDto>> ListAsync<TResultDto>(
        Expression<Func<TEntity, bool>>? preFilter,
        Expression<Func<TResultDto, bool>>? filter = null,
        Paging? paging = null,
        IReadOnlyCollection<Sorting<TResultDto>>? sortings = null,
        CancellationToken cancellationToken = default)
    {
        var entityQuery = await BuildListQueryAsync(preFilter).ConfigureAwait(false);
        var resultQuery = EntityTypeProjections.ProjectTo<TResultDto>(entityQuery, DbContext);
        resultQuery = CompleteListQuery(resultQuery, filter, sortings, paging);
        return await resultQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<TResultDto>> ListAsync<TResultDto>(
        Expression<Func<TEntity, TResultDto>> projection,
        Expression<Func<TEntity, bool>>? filter = null,
        Paging? paging = null,
        IReadOnlyCollection<Sorting<TEntity>>? sortings = null,
        CancellationToken cancellationToken = default)
    {
        var entityQuery = await BuildListQueryAsync(filter).ConfigureAwait(false);
        entityQuery = ApplySortings(entityQuery, sortings);
        var resultQuery = entityQuery.Select(projection);
        resultQuery = CompleteListQuery(resultQuery, null, null, paging);
        return await resultQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildListQueryAsync(filter).ConfigureAwait(false);
        query = CompleteListQuery(query);
        return await query.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> CountAsync<TResultDto>(
        Expression<Func<TEntity, bool>>? preFilter,
        Expression<Func<TResultDto, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var entityQuery = await BuildListQueryAsync(preFilter).ConfigureAwait(false);
        var resultQuery = EntityTypeProjections.ProjectTo<TResultDto>(entityQuery, DbContext);
        resultQuery = CompleteListQuery(resultQuery, filter);
        return await resultQuery.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildListQueryAsync(filter).ConfigureAwait(false);
        query = CompleteListQuery(query);
        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> AnyAsync<TResultDto>(
        Expression<Func<TEntity, bool>>? preFilter,
        Expression<Func<TResultDto, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var entityQuery = await BuildListQueryAsync(preFilter).ConfigureAwait(false);
        var resultQuery = EntityTypeProjections.ProjectTo<TResultDto>(entityQuery, DbContext);
        resultQuery = CompleteListQuery(resultQuery, filter);
        return await resultQuery.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    protected async Task<IQueryable<TEntity>> BuildListQueryAsync(
        Expression<Func<TEntity, bool>>? filter = default,
        Includes<TEntity>? includes = default)
    {
        var readDataAccessPolicy = await ReadDataAccessPolicy.Task.ConfigureAwait(false);
        var query = ApplyIncludes(DbSet.Value, includes);
        query = ApplyFiltering(query, filter);
        query = ApplyFiltering(query, readDataAccessPolicy?.CanBeAccessedCriteria);
        return query.AsNoTracking();
    }

    protected static IQueryable<T> CompleteListQuery<T>(
        IQueryable<T> query,
        Expression<Func<T, bool>>? filter = default,
        IReadOnlyCollection<Sorting<T>>? sortings = default,
        Paging? paging = default)
    {
        query = ApplyFiltering(query, filter);
        query = ApplySortings(query, sortings);
        query = ApplyPaging(query, paging);
        query = query.DecompileAsync();
        return query;
    }

    protected static IQueryable<T> ApplyFiltering<T>(IQueryable<T> query, Expression<Func<T, bool>>? filter)
    {
        return filter is null
            ? query
            : query.Where(filter);
    }

    protected static IQueryable<T> ApplySortings<T>(IQueryable<T> query, IReadOnlyCollection<Sorting<T>>? sortings)
    {
        if (sortings?.Count > 0)
        {
            var firstSorting = sortings.First();
            var sortedQuery = firstSorting.Direction switch
            {
                ListSortDirection.Ascending => query.OrderBy(firstSorting.SortBy),
                ListSortDirection.Descending => query.OrderByDescending(firstSorting.SortBy),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(sortings),
                    $"Unexpected sorting direction {firstSorting.Direction}")
            };
            sortedQuery = sortings.Skip(1)
                .Aggregate(
                    sortedQuery,
                    (result, sorting) =>
                        sorting.Direction switch
                        {
                            ListSortDirection.Ascending => result.ThenBy(sorting.SortBy),
                            ListSortDirection.Descending => result.ThenByDescending(sorting.SortBy),
                            _ => throw new ArgumentOutOfRangeException(
                                nameof(sortings),
                                $"Unexpected sorting direction {sorting.Direction}")
                        });
            return sortedQuery;
        }
        return query;
    }

    protected static IQueryable<T> ApplyPaging<T>(IQueryable<T> query, Paging? paging)
    {
        return paging is null
            ? query
            : query.Skip(paging.Skip).Take(paging.Take);
    }

    protected static IQueryable<TEntity> ApplyIncludes(
        IQueryable<TEntity> query,
        IEnumerable<Expression<Func<TEntity, object>>>? includes)
    {
        return includes is null
            ? query
            : includes
                .Aggregate(
                    query,
                    (current, include) => current.Include(include));
    }

    protected static IQueryable<TEntity> ApplyIncludes(
        IQueryable<TEntity> query,
        Includes<TEntity>? includes)
    {
        if (includes is null)
        {
            return query;
        }
        var includesTraits = includes.Replay(new IncludesTraits<TEntity>());
        query = includesTraits.HaveWhereClause
            ? includes.Replay(new EFPlusIncludes<TEntity>(query)).GetQuery() // Seems like EFCoreIncludes can be used all the time as they got support for Where
            : includes.Replay(new EFCoreIncludes<TEntity>(query)).GetQuery();
        query = includes.HaveCartesianExplosion
            ? query.AsSplitQuery()
            : query;
        return query;
    }
}

/// <inheritdoc/>
public class BaseRORepository<TDbContext, TEntity, TEntityId>
    : BaseRORepository<TDbContext, TEntity>
    , IRORepository<TEntity, TEntityId>
    where TDbContext : DbContext
    where TEntity : DomainEntity<TEntityId>
    where TEntityId : notnull
{
    public BaseRORepository(
        TDbContext dbContext,
        IRODataAccessPolicyProvider<TEntity> roDataAccessPolicyProvider,
        IEntityTypeProjections<TEntity> entityTypeProjections)
        : base(dbContext, roDataAccessPolicyProvider, entityTypeProjections)
    {
    }

    public async Task<TEntity> GetByIdAsync(
        TEntityId entityId,
        Includes<TEntity>? includes,
        CancellationToken cancellationToken)
    {
        var query = await BuildGetByIdQueryAsync(entityId, includes).ConfigureAwait(false);
        var entity = await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (entity == default)
        {
            await EnsureEntityExistsAsync(
                entityId,
                (entity) => entity.Id!.Equals(entityId),
                cancellationToken).ConfigureAwait(false);
        }
        return entity!;
    }

    public async Task<TResultDto> GetByIdAsync<TResultDto>(
        TEntityId entityId,
        CancellationToken cancellationToken)
    {
        var query = await BuildGetByIdQueryAsync(entityId, default).ConfigureAwait(false);
        var entityDto = await EntityTypeProjections.ProjectTo<TResultDto>(query, DbContext)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (EqualityComparer<TResultDto>.Default.Equals(entityDto, default!))
        {
            await EnsureEntityExistsAsync(
                entityId,
                (entity) => entity.Id!.Equals(entityId),
                cancellationToken).ConfigureAwait(false);
        }

        return entityDto!;
    }

    public async Task<TResultDto> GetByIdAsync<TResultDto>(
        TEntityId entityId,
        Expression<Func<TEntity, TResultDto>> projection,
        CancellationToken cancellationToken)
    {
        var query = await BuildGetByIdQueryAsync(entityId, default).ConfigureAwait(false);
        var entityDto = await query.Select(projection)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (EqualityComparer<TResultDto>.Default.Equals(entityDto, default!))
        {
            await EnsureEntityExistsAsync(
                entityId,
                (entity) => entity.Id!.Equals(entityId),
                cancellationToken).ConfigureAwait(false);
        }

        return entityDto!;
    }

    protected async Task<IQueryable<TEntity>> BuildGetByIdQueryAsync(
        TEntityId id,
        Includes<TEntity>? includes)
    {
        var readDataAccessPolicy = await ReadDataAccessPolicy.Task.ConfigureAwait(false);
        var query = ApplyIncludes(DbSet.Value, includes);
        query = ApplyFiltering(query, (e) => e.Id!.Equals(id));
        query = ApplyFiltering(query, readDataAccessPolicy?.CanBeAccessedCriteria);
        return query.AsNoTracking().DecompileAsync();
    }

    protected async Task EnsureEntityExistsAsync(
        TEntityId entityId,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        var readDataAccessPolicy = await ReadDataAccessPolicy.Task.ConfigureAwait(false);
        if (readDataAccessPolicy?.CanBeAccessedCriteria is null
            || !await DbSet.Value.AnyAsync(filter, cancellationToken).ConfigureAwait(false))
        {
            if (EqualityComparer<TEntityId>.Default.Equals(entityId, default!))
            {
                throw new EntityNotFoundException<TEntity, TEntityId>();
            }
            throw new EntityNotFoundException<TEntity, TEntityId>(entityId);
        }
        else
        {
            if (EqualityComparer<TEntityId>.Default.Equals(entityId, default!))
            {
                throw new EntityAccessDeniedException<TEntity, TEntityId>();
            }
            throw new EntityAccessDeniedException<TEntity, TEntityId>(entityId);
        }
    }
}
