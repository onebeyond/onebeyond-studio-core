using System.Collections.Generic;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

/// <summary>
/// Command for updating an aggregate root by DTO
/// </summary>
/// <typeparam name="TAggregateRootUpdateDto"></typeparam>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TAggregateRootId"></typeparam>
public record Update<TAggregateRootUpdateDto, TAggregateRoot, TAggregateRootId>
    : IAuditableRequest<TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
{
    /// <summary>
    /// </summary>
    public Update(
        TAggregateRootId aggregateRootId,
        TAggregateRootUpdateDto aggregateRootUpdateDto)
    {
        EnsureArg.IsFalse(
            EqualityComparer<TAggregateRootId>.Default.Equals(aggregateRootId, default!),
            nameof(aggregateRootId));
        EnsureArg.IsFalse(
            EqualityComparer<TAggregateRootUpdateDto>.Default.Equals(aggregateRootUpdateDto, default!),
            nameof(aggregateRootUpdateDto));

        AggregateRootId = aggregateRootId;
        AggregateRootUpdateDto = aggregateRootUpdateDto;
    }

    /// <summary>
    /// Id of an aggregate root to be updated
    /// </summary>
    public TAggregateRootId AggregateRootId { get; }

    /// <summary>
    /// DTO to be used for aggregate root updating
    /// </summary>
    public TAggregateRootUpdateDto AggregateRootUpdateDto { get; }
}
