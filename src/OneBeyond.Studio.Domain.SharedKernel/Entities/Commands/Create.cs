using System.Collections.Generic;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

/// <summary>
/// Command for creating an aggregate root by DTO
/// </summary>
/// <typeparam name="TAggregateRootCreateDto"></typeparam>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TAggregateRootId"></typeparam>
public record Create<TAggregateRootCreateDto, TAggregateRoot, TAggregateRootId>
    : IAuditableRequest<TAggregateRootId>
    where TAggregateRoot : DomainEntity<TAggregateRootId>, IAggregateRoot
{
    /// <summary>
    /// </summary>
    public Create(TAggregateRootCreateDto aggregateRootCreateDto)
    {
        EnsureArg.IsFalse(
            EqualityComparer<TAggregateRootCreateDto>.Default.Equals(aggregateRootCreateDto, default!),
            nameof(aggregateRootCreateDto));

        AggregateRootCreateDto = aggregateRootCreateDto;
    }

    /// <summary>
    /// DTO to be used for aggregate root creating
    /// </summary>
    public TAggregateRootCreateDto AggregateRootCreateDto { get; }
}
