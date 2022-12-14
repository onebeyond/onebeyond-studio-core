using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.EntityAuditing.Domain;

public interface IExtendedAuditReader<TEntity>
    where TEntity : class
{
    Task<List<TResultDTO>> GetAllEventsAsync<TResultDTO>(
        Expression<Func<TEntity, bool>> filter = null,
        Sorting<TEntity> sorting = null,
        Paging paging = null,
        CancellationToken cancellationToken = default)
    where TResultDTO : new();
}
