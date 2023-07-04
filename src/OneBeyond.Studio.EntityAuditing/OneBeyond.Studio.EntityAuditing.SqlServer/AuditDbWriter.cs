using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.EntityAuditing.Domain;
using OneBeyond.Studio.EntityAuditing.Domain.AuditEntityNameResolvers;
using OneBeyond.Studio.EntityAuditing.Domain.AuditEventSerializers;

namespace OneBeyond.Studio.EntityAuditing.SqlServer;

public class AuditDbWriter<TEntity> : IAuditWriter<TEntity>
    where TEntity : class
{
    private readonly IJsonAuditEventSerializer<TEntity> _auditStringBuilder;
    private readonly IAuditNameResolver<TEntity> _auditEntityTypeBuilder;
    private readonly IAuditEventRepository _repository;

    public AuditDbWriter(
        IJsonAuditEventSerializer<TEntity> auditStringBuilder,
        IAuditNameResolver<TEntity> auditEntityTypeBuilder,
        IAuditEventRepository repository)
    {
        EnsureArg.IsNotNull(auditStringBuilder);
        EnsureArg.IsNotNull(auditEntityTypeBuilder);
        EnsureArg.IsNotNull(repository);

        _auditStringBuilder = auditStringBuilder;
        _auditEntityTypeBuilder = auditEntityTypeBuilder;
        _repository = repository;
    }

    public virtual async Task WriteAsync(TEntity entity, AuditEvent auditEntityEvent, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(entity, nameof(entity));
        EnsureArg.IsNotNull(auditEntityEvent, nameof(auditEntityEvent));

        var changes = await _auditStringBuilder.SerializeAsync(entity, auditEntityEvent, cancellationToken);

        if (auditEntityEvent.EventType == AuditActionType.Update.Name && changes.IsEmpty)
        {
            return; // Do not write anything if there are no changes to record
        }

        var entityName = _auditEntityTypeBuilder.GetEntityName();

        var auditEvent = Entities.AuditEvent.FromAuditInfo(
            auditEntityEvent, 
            _auditEntityTypeBuilder.GetEntityName(), 
            changes.Data.ToString());

        await _repository.AddAsync(auditEvent, cancellationToken);
    }
}
