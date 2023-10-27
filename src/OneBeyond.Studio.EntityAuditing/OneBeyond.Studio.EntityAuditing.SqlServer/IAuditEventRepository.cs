using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.EntityAuditing.SqlServer.Entities;

namespace OneBeyond.Studio.EntityAuditing.SqlServer;

public interface IAuditEventRepository
{
    Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
    Task AddBulkAsync(IReadOnlyCollection<AuditEvent> auditEvents, CancellationToken cancellationToken = default);
}
