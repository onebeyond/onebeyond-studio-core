using Newtonsoft.Json.Linq;

namespace OneBeyond.Studio.EntityAuditing.Domain.AuditEventSerializers;

public interface IJsonAuditEventSerializer<TEntity>
    : IAuditEventSerializer<TEntity, JToken>
    where TEntity : class
{
}
