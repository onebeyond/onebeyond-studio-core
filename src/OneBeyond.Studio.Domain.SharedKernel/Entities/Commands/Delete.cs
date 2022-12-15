using System.Collections.Generic;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

/// <summary>
/// Command for deleting aggregate root by Id
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TAggregateRootId"></typeparam>
public record Delete<TAggregateRoot, TAggregateRootId>
    : IAuditableRequest<TAggregateRootId>
    where TAggregateRoot : DomainEntity<TAggregateRootId>, IAggregateRoot
{
    /// <summary>
    /// </summary>
    public Delete(TAggregateRootId aggregateRootId)
    {
        EnsureArg.IsFalse(
            EqualityComparer<TAggregateRootId>.Default.Equals(aggregateRootId, default!),
            nameof(aggregateRootId));

        AggregateRootId = aggregateRootId;
    }

    /// <summary>
    /// Id of an aggregate root to be deleted
    /// </summary>
    public TAggregateRootId AggregateRootId { get; }
}
