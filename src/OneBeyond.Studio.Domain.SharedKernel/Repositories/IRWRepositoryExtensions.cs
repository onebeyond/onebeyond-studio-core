using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Domain.SharedKernel.Repositories;

public static class IRWRepositoryExtensions
{
    public static Task<TAggregateRoot> GetByIdAsync<TAggregateRoot, TAggregateRootId>(
        this IRWRepository<TAggregateRoot, TAggregateRootId> rwRepository,
        TAggregateRootId aggregateRootId,
        CancellationToken cancellationToken)
        where TAggregateRoot : DomainEntity<TAggregateRootId>, IAggregateRoot
        where TAggregateRootId : notnull
    {
        EnsureArg.IsNotNull(rwRepository, nameof(rwRepository));

        return rwRepository.GetByIdAsync(
            aggregateRootId,
            default,
            cancellationToken);
    }

    public static Task<TAggregateRoot> GetByFilterAsync<TAggregateRoot, TAggregateRootId>(
        this IRWRepository<TAggregateRoot, TAggregateRootId> rwRepository,
        Expression<Func<TAggregateRoot, bool>> filter,
        CancellationToken cancellationToken)
        where TAggregateRoot : DomainEntity<TAggregateRootId>, IAggregateRoot
        where TAggregateRootId : notnull
    {
        EnsureArg.IsNotNull(rwRepository, nameof(rwRepository));

        return rwRepository.GetByFilterAsync(
            filter,
            default,
            cancellationToken);
    }
}
