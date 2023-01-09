using EnsureThat;
using Newtonsoft.Json;
using OneBeyond.Studio.Crosscuts.Json;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.Application.SharedKernel.DomainEvents;

/// <summary>
/// Serializes/deserializes domain events into/from JSON.
/// </summary>
public static class DomainEventSerializer
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new PrivateSetterContractResolver(),
        };

    /// <summary>
    /// Serializes domain event into JSON string.
    /// </summary>
    /// <param name="domainEvent"></param>
    /// <returns></returns>
    public static string SerializeToJson(DomainEvent domainEvent)
    {
        EnsureArg.IsNotNull(domainEvent, nameof(domainEvent));
        return JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings);
    }

    /// <summary>
    /// Deserializes domain event from JSON string.
    /// </summary>
    /// <param name="domainEventJson"></param>
    /// <returns></returns>
    public static DomainEvent DeserializeFromJson(string domainEventJson)
    {
        EnsureArg.IsNotNullOrWhiteSpace(domainEventJson, nameof(domainEventJson));
        return JsonConvert.DeserializeObject<DomainEvent>(domainEventJson, JsonSerializerSettings)!;
    }
}
