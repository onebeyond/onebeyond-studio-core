using EnsureThat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

/// <summary>
/// </summary>
public sealed record IntegrationEventEnvelope
{
    [JsonConstructor]
    internal IntegrationEventEnvelope(string typeName, string version, JObject data)
    {
        EnsureArg.IsNotNullOrWhiteSpace(typeName, nameof(typeName));
        EnsureArg.IsNotNullOrWhiteSpace(version, nameof(version));
        EnsureArg.IsNotNull(data, nameof(data));

        TypeName = typeName;
        Version = version;
        Data = data;
    }

    /// <summary>
    /// </summary>
    public string TypeName { get; }
    /// <summary>
    /// </summary>
    public string Version { get; }
    /// <summary>
    /// </summary>
    public JObject Data { get; }
}
