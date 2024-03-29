using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Application.SharedKernel.Repositories;

/// <summary>
/// Represents a repository with read-write operations.
/// </summary>
public interface IRWRepository<TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
    where TAggregateRootId : notnull
{
    Task<TAggregateRoot> GetByIdAsync(
        TAggregateRootId aggregateRootId,
        Includes<TAggregateRoot>? includes,
        CancellationToken cancellationToken);

    Task<TAggregateRoot> GetByFilterAsync(
        Expression<Func<TAggregateRoot, bool>> filter,
        Includes<TAggregateRoot>? includes,
        CancellationToken cancellationToken);

    Task CreateAsync(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        TAggregateRoot aggregateRoot,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        TAggregateRootId id,
        CancellationToken cancellationToken);
}
