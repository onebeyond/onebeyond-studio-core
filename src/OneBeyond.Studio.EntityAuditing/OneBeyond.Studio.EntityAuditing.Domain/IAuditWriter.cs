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
}
