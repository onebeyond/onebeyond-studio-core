using Newtonsoft.Json;

namespace OneBeyond.Studio.EntityAuditing.Domain;

public sealed class AuditChange
{
    [JsonProperty(Order = 10)]
    public string PropertyName { get; set; }

    [JsonProperty(Order = 20)]
    public string PropertyType { get; set; }

    [JsonProperty(Order = 30)]
    public object OriginalValue { get; set; }

    [JsonProperty(Order = 40)]
    public object NewValue { get; set; }

}
