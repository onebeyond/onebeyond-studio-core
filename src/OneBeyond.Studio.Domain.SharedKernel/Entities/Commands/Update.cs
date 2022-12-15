using System.Collections.Generic;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

/// <summary>
/// Command for updating an aggregate root by DTO
/// </summary>
/// <typeparam name="TAggregateRootUpdateDTO"></typeparam>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TAggregateRootId"></typeparam>
public record Update<TAggregateRootUpdateDTO, TAggregateRoot, TAggregateRootId>
    : IAuditableRequest<TAggregateRootId>
    where TAggregateRoot : DomainEntity<TAggregateRootId>, IAggregateRoot
{
    /// <summary>
    /// </summary>
    public Update(
        TAggregateRootId aggregateRootId,
        TAggregateRootUpdateDTO aggregateRootUpdateDTO)
    {
        EnsureArg.IsFalse(
            EqualityComparer<TAggregateRootId>.Default.Equals(aggregateRootId, default!),
            nameof(aggregateRootId));
        EnsureArg.IsFalse(
            EqualityComparer<TAggregateRootUpdateDTO>.Default.Equals(aggregateRootUpdateDTO, default!),
            nameof(aggregateRootUpdateDTO));

        AggregateRootId = aggregateRootId;
        AggregateRootUpdateDTO = aggregateRootUpdateDTO;
    }

    /// <summary>
    /// Id of an aggregate root to be updated
    /// </summary>
    public TAggregateRootId AggregateRootId { get; }

    /// <summary>
    /// DTO to be used for aggregate root updating
    /// </summary>
    public TAggregateRootUpdateDTO AggregateRootUpdateDTO { get; }
}
