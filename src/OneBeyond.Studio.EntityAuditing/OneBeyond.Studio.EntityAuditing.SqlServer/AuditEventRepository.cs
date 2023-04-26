using System;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using OneBeyond.Studio.EntityAuditing.SqlServer.Entities;

namespace OneBeyond.Studio.EntityAuditing.SqlServer;

internal sealed class AuditEventRepository : IAuditEventRepository
{
    private readonly DbContext _dbContext;
    private readonly Lazy<DbSet<AuditEvent>> _dbSet;

    public AuditEventRepository(AuditDbContext dbContext)
    {
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
        _dbSet = new Lazy<DbSet<AuditEvent>>(() => _dbContext.Set<AuditEvent>());
    }

    public Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNull(auditEvent, nameof(auditEvent));

        _dbSet.Value.Add(auditEvent);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
