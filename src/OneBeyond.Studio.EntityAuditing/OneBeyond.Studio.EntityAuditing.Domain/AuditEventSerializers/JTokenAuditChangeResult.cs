using Newtonsoft.Json.Linq;

namespace OneBeyond.Studio.EntityAuditing.Domain.AuditEventSerializers;

public sealed class JTokenAuditChangeResult : IAuditChangeResult<JToken>
{
    public JTokenAuditChangeResult(JToken data)
        => Data = data;

    public bool IsEmpty
        => Data.Type == JTokenType.Null;

    public JToken Data { get; private set; }
}
