using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.Domain.SharedKernel.Repositories;

public static class IRORepositoryExtensions
{
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

    public static Task<IReadOnlyCollection<TResultDTO>> ListAsync<TEntity, TEntityId, TResultDTO>(
        this IRORepository<TEntity, TEntityId> roRepository,
        Expression<Func<TResultDTO, bool>>? filter = default,
        Paging? paging = default,
        IReadOnlyCollection<Sorting<TResultDTO>>? sortings = default,
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

    public static Task<int> CountAsync<TEntity, TEntityId, TResultDTO>(
        this IRORepository<TEntity, TEntityId> roRepository,
        Expression<Func<TResultDTO, bool>>? filter = default,
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

    public static Task<bool> AnyAsync<TEntity, TEntityId, TResultDTO>(
        this IRORepository<TEntity, TEntityId> roRepository,
        Expression<Func<TResultDTO, bool>>? filter = default,
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
