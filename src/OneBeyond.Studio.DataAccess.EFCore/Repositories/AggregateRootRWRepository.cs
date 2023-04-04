using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using OneBeyond.Studio.Crosscuts.Reflection;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Repositories;

public class AggregateRootRWRepository<TDbContext, TAggregateRoot, TEntity, TEntityId>
    : IAggregateRootRWRepository<TAggregateRoot, TEntity, TEntityId>
    where TDbContext : DbContext
    where TAggregateRoot : AggregateRoot<TEntity, TEntityId>
    where TEntity : DomainEntity<TEntityId>
{
    private static readonly Lazy<Action<TAggregateRoot, IEnumerable<TEntity>>> AddEntities = new(CompileAddEntities);
    private static readonly Includes<TEntity> EmptyIncludes = new();

    private readonly HashSet<TEntity> _entities;
    private readonly Lazy<TAggregateRoot> _aggregateRoot;

    public AggregateRootRWRepository(TDbContext dbContext)
    {
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));

        _entities = new HashSet<TEntity>();
        _aggregateRoot = new Lazy<TAggregateRoot>(() => Activator.CreateInstance<TAggregateRoot>());
        DbContext = dbContext;
    }

    protected TDbContext DbContext { get; }

    /// <inheritdoc/>
    public Task<TAggregateRoot> GetAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
        => GetAsync(filter, EmptyIncludes, cancellationToken);

    /// <inheritdoc/>
    public async Task<TAggregateRoot> GetAsync(
        Expression<Func<TEntity, bool>> filter,
        Includes<TEntity> includes,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(filter, nameof(filter));
        EnsureArg.IsNotNull(includes, nameof(includes));

        var aggregateRoot = _aggregateRoot.Value;

        var entityQuery = ApplyFilter(DbContext.Set<TEntity>(), filter);

        entityQuery = ApplyIncludes(entityQuery, includes);

        var entities = await entityQuery.ToArrayAsync(cancellationToken).ConfigureAwait(false);

        var entitiesToAdd = entities.Except(_entities).ToArray();

        AddEntities.Value(aggregateRoot, entitiesToAdd);

        _entities.UnionWith(entitiesToAdd);

        return aggregateRoot;
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken)
    {
        EnsureArg.IsTrue(
            _aggregateRoot.IsValueCreated && ReferenceEquals(_aggregateRoot.Value, aggregateRoot),
            nameof(aggregateRoot));

        var entitiesToRemove = _entities.Except(aggregateRoot.Entities).ToArray();
        var entitiesToAdd = aggregateRoot.Entities.Except(_entities).ToArray();

        DbContext.Set<TEntity>().RemoveRange(_entities.Except(aggregateRoot.Entities));
        DbContext.Set<TEntity>().AddRange(aggregateRoot.Entities.Except(_entities));

        _entities.ExceptWith(entitiesToRemove);
        _entities.UnionWith(entitiesToAdd);

        try
        {
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            _entities.UnionWith(entitiesToRemove);
            _entities.ExceptWith(entitiesToAdd);
            throw;
        }
    }

    protected virtual Task SaveChangesAsync(CancellationToken cancellationToken)
        => DbContext.SaveChangesAsync(cancellationToken);

    protected static IQueryable<TEntity> ApplyFilter(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>>? filter)
        => filter is null
            ? query
            : query.Where(filter);

    protected static IQueryable<TEntity> ApplyIncludes(
        IQueryable<TEntity> query,
        Includes<TEntity> includes)
    {
        if (includes == EmptyIncludes)
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

    protected static Action<TAggregateRoot, IEnumerable<TEntity>> CompileAddEntities()
    {
        var entitiesFieldInfo = typeof(AggregateRoot<TEntity, TEntityId>).GetField(
            "_entities",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var addRangeMethodInfo = Reflector.MethodFrom(
            (List<TEntity> list) => list.AddRange(default!));
        var entitiesExpresison = Expression.Parameter(
            typeof(IEnumerable<TEntity>),
            "entities");
        var aggregateRootExpression = Expression.Parameter(
            typeof(TAggregateRoot),
            "aggregateRoot");
        var entityListExpression = Expression.MakeMemberAccess(
            aggregateRootExpression,
            entitiesFieldInfo!);
        var addRangeExpression = Expression.Call(
            entityListExpression,
            addRangeMethodInfo,
            entitiesExpresison);
        var addEntitiesExpression = Expression.Lambda<Action<TAggregateRoot, IEnumerable<TEntity>>>(
            addRangeExpression,
            aggregateRootExpression,
            entitiesExpresison);
        return addEntitiesExpression.Compile();
    }
}
