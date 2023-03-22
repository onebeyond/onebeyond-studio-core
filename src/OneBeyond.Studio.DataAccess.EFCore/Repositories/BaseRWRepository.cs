using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DelegateDecompiler.EntityFrameworkCore;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using OneBeyond.Studio.Application.SharedKernel.DataAccessPolicies;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Application.SharedKernel.Repositories.Exceptions;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using OneBeyond.Studio.DataAccess.EFCore.Projections;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Repositories;

/// <summary>
/// Implements base read-write repository for the <typeparamref name="TAggregateRoot"/> type using EF Core.
/// All the entities returned by this repository are trackable.
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TAggregateRootId"></typeparam>
public class BaseRWRepository<TDbContext, TAggregateRoot, TAggregateRootId>
    : BaseRORepository<TDbContext, TAggregateRoot, TAggregateRootId>
    , IRWRepository<TAggregateRoot, TAggregateRootId>
    where TDbContext : DbContext
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
    where TAggregateRootId : notnull
{
    public BaseRWRepository(
        TDbContext dbContext,
        IRWDataAccessPolicyProvider<TAggregateRoot> rwDataAccessPolicyProvider,
        IEntityTypeProjections<TAggregateRoot> entityTypeProjections)
        : base(dbContext, rwDataAccessPolicyProvider, entityTypeProjections)
    {
        EnsureCreateDataAccessPolicy = new AsyncLazy<Action<TAggregateRoot>>(
            async () =>
                CompileDataAccessFunction(
                    await rwDataAccessPolicyProvider
                        .GetCreateDataAccessPolicyAsync()
                        .ConfigureAwait(false)),
            AsyncLazyFlags.RetryOnFailure);
        EnsureUpdateDataAccessPolicy = new AsyncLazy<Action<TAggregateRoot>>(
            async () =>
                CompileDataAccessFunction(
                    await rwDataAccessPolicyProvider
                        .GetUpdateDataAccessPolicyAsync()
                        .ConfigureAwait(false)),
            AsyncLazyFlags.RetryOnFailure);
        EnsureDeleteDataAccessPolicy = new AsyncLazy<Action<TAggregateRoot>>(
            async () =>
                CompileDataAccessFunction(
                    await rwDataAccessPolicyProvider
                        .GetDeleteDataAccessPolicyAsync()
                        .ConfigureAwait(false)),
            AsyncLazyFlags.RetryOnFailure);
    }

    protected AsyncLazy<Action<TAggregateRoot>> EnsureCreateDataAccessPolicy { get; }
    protected AsyncLazy<Action<TAggregateRoot>> EnsureUpdateDataAccessPolicy { get; }
    protected AsyncLazy<Action<TAggregateRoot>> EnsureDeleteDataAccessPolicy { get; }

    public new async Task<TAggregateRoot> GetByIdAsync(
        TAggregateRootId aggregateRootId,
        Includes<TAggregateRoot>? includes,
        CancellationToken cancellationToken)
    {
        var query = await BuildGetQueryAsync(
            (aggregateRoot) => aggregateRoot.Id!.Equals(aggregateRootId),
            includes).ConfigureAwait(false);
        var entity = await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            await EnsureEntityExistsAsync(
                aggregateRootId,
                (aggregateRoot) => aggregateRoot.Id!.Equals(aggregateRootId),
                cancellationToken).ConfigureAwait(false);
        }
        return entity!;
    }

    public async Task<TAggregateRoot> GetByFilterAsync(
        Expression<Func<TAggregateRoot, bool>> filter,
        Includes<TAggregateRoot>? includes,
        CancellationToken cancellationToken)
    {
        var query = await BuildGetQueryAsync(filter, includes).ConfigureAwait(false);
        var entity = await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            await EnsureEntityExistsAsync(default!, filter, cancellationToken).ConfigureAwait(false);
        }
        return entity!;
    }

    public async Task CreateAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(aggregateRoot, nameof(aggregateRoot));
        var ensureCreateDataAccessPolicy = await EnsureCreateDataAccessPolicy.Task.ConfigureAwait(false);
        ensureCreateDataAccessPolicy(aggregateRoot);
        DbSet.Value.Add(aggregateRoot);
        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(aggregateRoot, nameof(aggregateRoot));
        var ensureUpdateDataAccessPolicy = await EnsureUpdateDataAccessPolicy.Task.ConfigureAwait(false);
        ensureUpdateDataAccessPolicy(aggregateRoot);
        DbSet.Value.Update(aggregateRoot);
        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(aggregateRoot, nameof(aggregateRoot));
        var ensureDeleteDataAccessPolicy = await EnsureDeleteDataAccessPolicy.Task.ConfigureAwait(false);
        ensureDeleteDataAccessPolicy(aggregateRoot);
        DbSet.Value.Remove(aggregateRoot);
        await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(TAggregateRootId id, CancellationToken cancellationToken)
    {
        var aggregate = DbSet.Value.Find(id);

        return aggregate == default
            ? Task.CompletedTask
            : DbSet.Value.DeleteByKeyAsync(cancellationToken, id);
    }

    protected virtual Task SaveChangesAsync(CancellationToken cancellationToken)
        => DbContext.SaveChangesAsync(cancellationToken);

    protected async Task<IQueryable<TAggregateRoot>> BuildGetQueryAsync(
        Expression<Func<TAggregateRoot, bool>> filter,
        Includes<TAggregateRoot>? includes)
    {
        var readDataAccessPolicy = await ReadDataAccessPolicy.Task.ConfigureAwait(false);
        var query = ApplyIncludes(DbSet.Value, includes);
        query = ApplyFiltering(query, filter);
        query = ApplyFiltering(query, readDataAccessPolicy?.CanBeAccessedCriteria);
        return query.DecompileAsync();
    }

    private static Action<TAggregateRoot> CompileDataAccessFunction(DataAccessPolicy<TAggregateRoot>? dataAccessPolicy)
    {
        var isDataAccessAllowed1 = dataAccessPolicy?.CanBeAccessedCriteria?.Compile();
        var isDataAccessAllowed2 = dataAccessPolicy?.CanBeAccessedFunction;
        if (isDataAccessAllowed1 is null
            && isDataAccessAllowed2 is null)
        {
            return (aggregateRoot) =>
            {
            };
        }
        if (isDataAccessAllowed1 is not null
            && isDataAccessAllowed2 is not null)
        {
            return (aggregateRoot) =>
            {
                if (isDataAccessAllowed1(aggregateRoot) && isDataAccessAllowed2(aggregateRoot))
                {
                    return;
                }
                throw new EntityAccessDeniedException<TAggregateRoot, TAggregateRootId>(aggregateRoot.Id);
            };
        }
        var isDataAccessAllowed = isDataAccessAllowed1
            ?? isDataAccessAllowed2;
        return (aggregateRoot) =>
        {
            if (isDataAccessAllowed!(aggregateRoot))
            {
                return;
            }
            throw new EntityAccessDeniedException<TAggregateRoot, TAggregateRootId>(aggregateRoot.Id);
        };
    }
}
