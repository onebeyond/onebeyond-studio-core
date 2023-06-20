using EnsureThat;
using Newtonsoft.Json;
using OneBeyond.Studio.ChangeTracker.Domain.Attributes;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.ChangeTracker.Domain.Entities;

public sealed class ChangeTrackerEvent : DomainEvent
{
    [JsonConstructor]
    public ChangeTrackerEvent(
        string entityId,
        string entityType,
        string entityTypeFullName,
        string changeType,
        DateTimeOffset dateRaised)
        : base(dateRaised)
    {
        EnsureArg.IsNotNullOrWhiteSpace(entityId, nameof(entityId));
        EnsureArg.IsNotNullOrWhiteSpace(entityType, nameof(entityType));
        EnsureArg.IsNotNullOrWhiteSpace(entityTypeFullName, nameof(entityTypeFullName));
        EnsureArg.IsNotNullOrWhiteSpace(changeType, nameof(changeType));

        EntityId = entityId;
        EntityType = entityType;
        EntityTypeFullName = entityTypeFullName;
        ChangeType = changeType;
    }

    public ChangeTrackerEvent(
        DomainEntity entity,
        object entityId,
        Type entityType,
        string changeType)
        : base(DateTimeOffset.UtcNow)
    {
        EnsureArg.IsNotNull(entityId, nameof(entity));
        EnsureArg.IsNotNull(entityType, nameof(entityType));
        EnsureArg.IsNotNull(changeType, nameof(changeType));

        EntityTypeFullName = entity.GetType().FullName!;
        EntityId = entityId.ToString()!; 
        EntityType = GetEntityType(entityType);
        ChangeType = changeType;
    }

    public string EntityId { get; private set; } 
    public string EntityType { get; }
    public string EntityTypeFullName { get; }
    public string ChangeType { get; }
    public List<PropertyChange> Changes { get; } = new();
    public List<ChangeTrackerEvent> Children { get; } = new();


    public void ChangeEntityId(object entityId)
        => EntityId = entityId.ToString()!; 

    public void AddChanges(params PropertyChange[] changes)
        => Changes.AddRange(changes);

    public void AddChild(ChangeTrackerEvent child)
        => Children.Add(child);

    private static string GetEntityType(Type type)
    {
        var auditEntityNameAttributes = type
            .GetCustomAttributes(typeof(ChangeTrackerEntityNameAttribute), true)
            .Cast<ChangeTrackerEntityNameAttribute>()
            .ToList();

        return auditEntityNameAttributes.Any()
            ? auditEntityNameAttributes[0].Name
            : type.Name;
    }
}
