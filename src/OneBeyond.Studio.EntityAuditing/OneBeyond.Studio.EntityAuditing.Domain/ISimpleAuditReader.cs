using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.EntityAuditing.Domain;

public interface ISimpleAuditReader<TEntity, TEntityId>
    where TEntity : class
{
    Task<List<TResultDTO>> GetEventsByIdAsync<TResultDTO>(
        TEntityId id,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        Paging paging = null,
        CancellationToken cancellationToken = default)
     where TResultDTO : new();

    Task<List<TResultDTO>> GetAllEventsAsync<TResultDTO>(
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        Paging paging = null,
        CancellationToken cancellationToken = default)
    where TResultDTO : new();
}
