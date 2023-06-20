using System.Collections.Generic;
using System.Diagnostics;
using EnsureThat;
using Newtonsoft.Json;
using OneBeyond.Studio.Application.SharedKernel.AmbientContexts;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Application.SharedKernel.DomainEvents;

/// <summary>
/// This class is used as an envelope for a domain event and its all accompanying data.
/// Probably it should've been named as DomainEventEnvelope.
/// </summary>
public sealed class RaisedDomainEvent
{
    public RaisedDomainEvent(
        DomainEntity domainEntity,
        DomainEvent domainEvent,
        AmbientContext? ambientContext,
        Activity? activity)
        :this(
             domainEntity.IdAsString, 
             domainEntity.GetType().FullName!, 
             domainEvent, 
             ambientContext, 
             activity)
    {
    }

    //Used by Change Tracker
    public RaisedDomainEvent(
        string entityId,
        string entityType,
        DomainEvent domainEvent,
        AmbientContext? ambientContext,
        Activity? activity)
    {
        EnsureArg.IsNotNullOrWhiteSpace(entityId, nameof(entityId));
        EnsureArg.IsNotNullOrWhiteSpace(entityType, nameof(entityType));
        EnsureArg.IsNotNull(domainEvent, nameof(domainEvent));

        EntityId = entityId;
        EntityType = entityType;
        DomainEventJson = DomainEventSerializer.SerializeToJson(domainEvent);
        AmbientContextJson = ambientContext is not null
                ? JsonConvert.SerializeObject(ambientContext)
                : default;
        ActivityId = activity?.Id;
        ActivityTraceState = activity?.TraceStateString;
    }

    [JsonConstructor]
    [System.Text.Json.Serialization.JsonConstructor]
    public RaisedDomainEvent(
        long id,
        string entityId,
        string entityType,
        string domainEventJson,
        string? ambientContextJson,
        string? activityId,
        string? activityTraceState)
    {
        EnsureArg.IsNotDefault(id, nameof(id));
        EnsureArg.IsNotNullOrWhiteSpace(entityId, nameof(entityId));
        EnsureArg.IsNotNullOrWhiteSpace(entityType, nameof(entityType));
        EnsureArg.IsNotNullOrWhiteSpace(domainEventJson, nameof(domainEventJson));

        Id = id;
        EntityId = entityId;
        EntityType = entityType;
        DomainEventJson = domainEventJson;
        AmbientContextJson = ambientContextJson;
        ActivityId = activityId;
        ActivityTraceState = activityTraceState;
    }

    public long Id { get; }
    public string EntityId { get; }
    public string EntityType { get; }
    public string DomainEventJson { get; }
    public string? AmbientContextJson { get; }
    public string? ActivityId { get; }
    public string? ActivityTraceState { get; }

    public RaisedDomainEvent Clone(Activity? activity)
        => new(
            Id,
            EntityId,
            EntityType,
            DomainEventJson,
            AmbientContextJson,
            activity?.Id,
            activity?.TraceStateString);

    public DomainEvent GetValue()
        => DomainEventSerializer.DeserializeFromJson(Ensure.Any.IsNotNull(DomainEventJson, nameof(DomainEventJson)));

    public IReadOnlyDictionary<string, object>? GetAmbientContext()
        => AmbientContextJson is not null
            ? JsonConvert.DeserializeObject<IReadOnlyDictionary<string, object>>(AmbientContextJson)
            : default;
}
