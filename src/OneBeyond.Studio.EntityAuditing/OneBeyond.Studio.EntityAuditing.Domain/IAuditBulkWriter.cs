using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.EntityAuditing.Domain;

public interface IAuditBulkWriter
{
    Task WriteAsync(object entity, AuditEvent @event, CancellationToken cancellationToken);

    Task FlushAsync(CancellationToken cancellationToken);
}
