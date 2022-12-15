using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.EntityAuditing.Domain;

namespace OneBeyond.Studio.EntityAuditing.Domain.AuditEventSerializers;

public interface IAuditEventSerializer<TEntity, TResult>
    where TEntity : class
    where TResult : class
{
    ValueTask<IAuditChangeResult<TResult>> SerializeAsync(TEntity entity, AuditEvent auditEntityEvent, CancellationToken cancellationToken);
}
