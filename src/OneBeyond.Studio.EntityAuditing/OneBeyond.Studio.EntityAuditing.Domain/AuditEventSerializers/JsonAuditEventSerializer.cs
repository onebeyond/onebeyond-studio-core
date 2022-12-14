using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Newtonsoft.Json.Linq;
using OneBeyond.Studio.EntityAuditing.Domain;

namespace OneBeyond.Studio.EntityAuditing.Domain.AuditEventSerializers;

public class JsonAuditEventSerializer<TEntity> : IJsonAuditEventSerializer<TEntity>
    where TEntity : class
{
    ValueTask<IAuditChangeResult<JToken>> IAuditEventSerializer<TEntity, JToken>.SerializeAsync(TEntity entity, AuditEvent auditEntityEvent, CancellationToken cancellationToken)
        => SerializeAsync(entity, auditEntityEvent, cancellationToken);

    public virtual ValueTask<IAuditChangeResult<JToken>> SerializeAsync(TEntity entity, AuditEvent auditEntityEvent, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(entity, nameof(entity));
        EnsureArg.IsNotNull(auditEntityEvent, nameof(auditEntityEvent));

        return new ValueTask<IAuditChangeResult<JToken>>(
            new JTokenAuditChangeResult(
                auditEntityEvent.EntityChanges != null && auditEntityEvent.EntityChanges.Any()
                ? JToken.FromObject(auditEntityEvent.EntityChanges)
                : JValue.CreateNull()));
    }
}
