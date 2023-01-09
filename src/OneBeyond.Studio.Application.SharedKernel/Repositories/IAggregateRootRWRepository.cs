using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Application.SharedKernel.Repositories;

public interface IAggregateRootRWRepository<TAggregateRoot, TEntity, TEntityId>
    where TAggregateRoot : AggregateRoot<TEntity, TEntityId>
    where TEntity : DomainEntity<TEntityId>
{
    /// <summary>
    /// </summary>
    /// <param name="filter">
    /// Defines entities which get loaded into the <see cref="AggregateRoot{TEntity, TEntityId}.Entities"/> collection.
    /// Usually you want to load just the ones which are required for carrying out a command.
    /// <para>
    /// This filter does not guarantee that the <see cref="AggregateRoot{TEntity, TEntityId}.Entities"/> collection will
    /// contain only those entities which satisfy it. The collection can contain entities loaded before in the same transaction
    /// scope.
    /// </para>
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<TAggregateRoot> GetAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="filter">
    /// Defines entities which get loaded into the <see cref="AggregateRoot{TEntity, TEntityId}.Entities"/> collection.
    /// Usually you want to load just the ones which are required for carrying out a command.
    /// <para>
    /// This filter does not guarantee that the <see cref="AggregateRoot{TEntity, TEntityId}.Entities"/> collection will
    /// contain only those entities which satisfy it. The collection can contain entities loaded before in the same transaction
    /// scope.
    /// </para>
    /// </param>
    /// <param name="includes">
    /// Defines additional entities which get loaded along with the <typeparamref name="TEntity"/> one via
    /// navigation properties of the latter.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<TAggregateRoot> GetAsync(
        Expression<Func<TEntity, bool>> filter,
        Includes<TEntity> includes,
        CancellationToken cancellationToken);

    public Task UpdateAsync(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken);
}
