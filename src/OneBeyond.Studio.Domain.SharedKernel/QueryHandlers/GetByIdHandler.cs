using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using MediatR;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Queries;
using OneBeyond.Studio.Domain.SharedKernel.Repositories;

namespace OneBeyond.Studio.Domain.SharedKernel.QueryHandlers;

/// <summary>
/// Handles <see cref="GetById{TResultDTO, TEntity, TEntityId}"/> query
/// </summary>
/// <typeparam name="TResultDTO"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TEntityId"></typeparam>
public class GetByIdHandler<TResultDTO, TEntity, TEntityId>
    : IRequestHandler<GetById<TResultDTO, TEntity, TEntityId>, TResultDTO>
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
    public Task<TResultDTO> Handle(
        GetById<TResultDTO, TEntity, TEntityId> query,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(query, nameof(query));

        return HandleAsync(query, cancellationToken);
    }

    /// <summary>
    /// </summary>
    protected virtual Task<TResultDTO> HandleAsync(
        GetById<TResultDTO, TEntity, TEntityId> query,
        CancellationToken cancellationToken)
        => RoRepository.GetByIdAsync<TResultDTO>(query.EntityId, cancellationToken);
}
