using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.EntityAuditing.Domain;

public interface IAuditBulkWriter
{
    Task WriteAsync<TEntity>(TEntity entity, AuditEvent @event, CancellationToken cancellationToken)
            where TEntity : class;

    Task FlushAsync(CancellationToken cancellationToken);
}
