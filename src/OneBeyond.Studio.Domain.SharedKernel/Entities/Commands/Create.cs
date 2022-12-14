using System.Collections.Generic;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

/// <summary>
/// Command for creating an aggregate root by DTO
/// </summary>
/// <typeparam name="TAggregateRootCreateDTO"></typeparam>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TAggregateRootId"></typeparam>
public record Create<TAggregateRootCreateDTO, TAggregateRoot, TAggregateRootId>
    : IAuditableRequest<TAggregateRootId>
    where TAggregateRoot : DomainEntity<TAggregateRootId>, IAggregateRoot
{
    /// <summary>
    /// </summary>
    public Create(TAggregateRootCreateDTO aggregateRootCreateDTO)
    {
        EnsureArg.IsFalse(
            EqualityComparer<TAggregateRootCreateDTO>.Default.Equals(aggregateRootCreateDTO, default!),
            nameof(aggregateRootCreateDTO));

        AggregateRootCreateDTO = aggregateRootCreateDTO;
    }

    /// <summary>
    /// DTO to be used for aggregate root creating
    /// </summary>
    public TAggregateRootCreateDTO AggregateRootCreateDTO { get; }
}
