using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.EntityAuditing.Domain;
using OneBeyond.Studio.EntityAuditing.Domain.AuditEntityNameResolvers;
using OneBeyond.Studio.EntityAuditing.Domain.AuditEventSerializers;

namespace OneBeyond.Studio.EntityAuditing.SqlServer;

public interface IAuditDbConverter
{
}

public class AuditDbConverter<TEntity> : IAuditDbConverter
    where TEntity : class
{
    private readonly IJsonAuditEventSerializer<TEntity> _auditStringBuilder;
    private readonly IAuditNameResolver<TEntity> _auditEntityTypeBuilder;

    public AuditDbConverter(
        IJsonAuditEventSerializer<TEntity> auditStringBuilder,
        IAuditNameResolver<TEntity> auditEntityTypeBuilder,
        IAuditEventRepository repository)
    {
        EnsureArg.IsNotNull(auditStringBuilder);
        EnsureArg.IsNotNull(auditEntityTypeBuilder);
        EnsureArg.IsNotNull(repository);

        _auditStringBuilder = auditStringBuilder;
        _auditEntityTypeBuilder = auditEntityTypeBuilder;
    }

    public async Task<Entities.AuditEvent> ConvertAsync(TEntity entity, AuditEvent auditEntityEvent, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(entity, nameof(entity));
        EnsureArg.IsNotNull(auditEntityEvent, nameof(auditEntityEvent));

        var changes = await _auditStringBuilder.SerializeAsync(entity, auditEntityEvent, cancellationToken);

        if (auditEntityEvent.EventType == AuditActionType.Update.Name && changes.IsEmpty)
        {
            return null; // Do not write anything if there are no changes to record
        }

        return Entities.AuditEvent.FromAuditInfo(
            auditEntityEvent,
            _auditEntityTypeBuilder.GetEntityName(),
            changes.Data.ToString());
    }
}

