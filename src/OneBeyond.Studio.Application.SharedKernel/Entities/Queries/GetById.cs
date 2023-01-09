using System.Collections.Generic;
using EnsureThat;
using MediatR;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Application.SharedKernel.Entities.Queries;

/// <summary>
/// Query for getting an entity projected by Id and projected to a DTO
/// </summary>
/// <typeparam name="TResultDTO"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TEntityId"></typeparam>
public record GetById<TResultDTO, TEntity, TEntityId>
    : IRequest<TResultDTO>
    where TEntity : DomainEntity<TEntityId>
{
    /// <summary>
    /// </summary>
    public GetById(TEntityId entityId)
    {
        EnsureArg.IsFalse(
            EqualityComparer<TEntityId>.Default.Equals(entityId, default!),
            nameof(entityId));

        EntityId = entityId;
    }

    /// <summary>
    /// Id of an entity to be get
    /// </summary>
    public TEntityId EntityId { get; }
}
