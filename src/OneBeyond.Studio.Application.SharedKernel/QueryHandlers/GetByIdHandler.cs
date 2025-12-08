using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.Entities.Queries;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Core.Mediator.Queries;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Application.SharedKernel.QueryHandlers;

/// <summary>
/// Handles <see cref="GetById{TResultDto, TEntity, TEntityId}"/> query
/// </summary>
/// <typeparam name="TResultDto"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TEntityId"></typeparam>
public class GetByIdHandler<TResultDto, TEntity, TEntityId>
    : IQueryHandler<GetById<TResultDto, TEntity, TEntityId>, TResultDto>
    where TEntity : DomainEntity<TEntityId>
    where TEntityId : notnull
{
    /// <summary>
    /// </summary>
    public GetByIdHandler(IRORepository<TEntity, TEntityId> roRepository)
    {
        EnsureArg.IsNotNull(roRepository, nameof(roRepository));

        RoRepository = roRepository;
    }

    /// <summary>
    /// </summary>
    protected IRORepository<TEntity, TEntityId> RoRepository { get; }

    /// <summary>
    /// </summary>
    public Task<TResultDto> HandleAsync(
        GetById<TResultDto, TEntity, TEntityId> query,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(query, nameof(query));

        return HandleGetByIdAsync(query, cancellationToken);
    }

    /// <summary>
    /// </summary>
    protected virtual Task<TResultDto> HandleGetByIdAsync(
        GetById<TResultDto, TEntity, TEntityId> query,
        CancellationToken cancellationToken)
        => RoRepository.GetByIdAsync<TResultDto>(query.EntityId, cancellationToken);
}
