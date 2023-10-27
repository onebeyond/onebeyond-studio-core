using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.EntityAuditing.Domain;

public interface IAuditWriter
{
}

public interface IAuditWriter<in TEntity> : IAuditWriter
    where TEntity : class
{
    Task WriteAsync(TEntity entity, AuditEvent @event, CancellationToken cancellationToken);
    Task WriteBulkAsync<T>(IReadOnlyCollection<(T Entity, AuditEvent Event)> entires, CancellationToken cancellationToken)
        where T : TEntity;
}
