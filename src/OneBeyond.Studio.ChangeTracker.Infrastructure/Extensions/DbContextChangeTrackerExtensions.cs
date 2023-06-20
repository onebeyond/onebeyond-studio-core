using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OneBeyond.Studio.ChangeTracker.Domain.Attributes;
using OneBeyond.Studio.ChangeTracker.Domain.Entities;
using OneBeyond.Studio.ChangeTracker.Infrastructure.Helpers;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.ChangeTracker.Infrastructure.Extensions;

public static class DbContextChangeTrackerExtensions
{
    private static readonly Dictionary<EntityState, string> _entityStateToAuditLogChangeTypeMapping = new()
    {
        { EntityState.Added, ChangeTypes.Created },
        { EntityState.Deleted, ChangeTypes.Deleted },
        { EntityState.Modified, ChangeTypes.Updated },
        { EntityState.Unchanged, ChangeTypes.Updated } // consider changed children as "modified"
    };

    private static readonly List<string> _defaultIgnoredProperties = new()
    {
        "Discriminator",
        "Discriminator",
        "Identity"
    };

    public static IReadOnlyList<ChangeTrackerEvent> GetEntityChanges<TDbContext>(
        this TDbContext dbContext)
        where TDbContext : DbContext
        => GetEntityChanges<DomainEntity, TDbContext>(dbContext);

    public static IReadOnlyList<ChangeTrackerEvent> GetEntityChanges<TAggregateRoot, TDbContext>(
        this TDbContext dbContext)
        where TAggregateRoot : DomainEntity
        where TDbContext : DbContext
    {
        var logs = new List<ChangeTrackerEvent>();
        try
        {
            var entries = dbContext.ChangeTracker.Entries<TAggregateRoot>()
                .Where(en =>
                        !en.Entity.GetType().IsDefined(typeof(ChangeTrackerIgnoreAttribute), false) &&
                        en.Entity.GetType().InheritsFromGenericType(typeof(AggregateRoot<>))) // Audit only aggregate roots
                .ToList();

            // disable change tracking for a significant performance improvement
            // this is safe in this scope as we're only adding new records and don't need change tracking
            dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var entry in entries)
            {
                var hasChanged = entry.State.IsChanged();
                if (!hasChanged && !GetChangedCollectionsAndChildren(dbContext, entry).Any())
                {
                    continue;
                }

                var entityAuditEvent = GetDeepChangesInline(dbContext, entry, parent: null);

                // NOTE: normally the condition should always be met (otherwise the AggregateRoot would have not been in the ChangeTracker),
                // however the event might be empty if developers have misunderstood the DDD approach and has changed an AR2 from within another
                // AR1. In that case, the change tracker would track the source AR1, but no change will be effectively audited on that AR1
                // as they will all be in the AR2 anyway.
                if (entityAuditEvent != null
                    && (entityAuditEvent.Changes.Any() ||
                        entityAuditEvent.Children.Any() ||
                        entityAuditEvent.ChangeType == ChangeTypes.Created || entityAuditEvent.ChangeType == ChangeTypes.Deleted))
                // This last condition covers the edge case where only Id and/or foreign Key were present in the entity/child 
                {
                    logs.Add(entityAuditEvent);
                }
            }
            return logs;
        }
        finally
        {
            dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }

    private static IEnumerable<PropertyChange> GetValueObjectChangesInline(
        string propertyName,
        IEnumerable<EntityEntry<ValueObject>> valueObjects)
    {
        // Value Objects are immutable, so they are NOT modified, they can be:
        // 1) added if not there
        // 2) deleted if there
        // 3) deleted and re-added with different values
        var addedValueObject = valueObjects.FirstOrDefault(vo => vo.State == EntityState.Added);
        var deletedValueObject = valueObjects.FirstOrDefault(vo => vo.State == EntityState.Deleted);

        if (addedValueObject is { } && deletedValueObject is null)
        {
            var props = addedValueObject.Properties;
            foreach (var prop in props)
            {
                if (!ShouldIgnoreProp(addedValueObject.Entity.GetType(), prop) && prop.CurrentValue != null)
                {
                    yield return CreateAuditChange(addedValueObject, propertyName, prop, originalValue: null, currentValue: prop.CurrentValue);
                }
            }
        }

        if (addedValueObject is null && deletedValueObject is { })
        {
            var props = deletedValueObject.Properties;
            foreach (var prop in props)
            {
                if (!ShouldIgnoreProp(deletedValueObject.Entity.GetType(), prop) && prop.OriginalValue != null)
                {
                    yield return CreateAuditChange(deletedValueObject, propertyName, prop, originalValue: prop.OriginalValue, currentValue: null);
                }
            }
        }

        if (addedValueObject is { } && deletedValueObject is { } && !addedValueObject.Equals(deletedValueObject))
        {
            var props = addedValueObject.Properties;
            foreach (var prop in props)
            {
                var deletedProp = deletedValueObject.Properties.FirstOrDefault(dp => dp.Metadata.Name == prop.Metadata.Name);
                if (!ShouldIgnoreProp(deletedValueObject.Entity.GetType(), prop) && !ArePropertiesEqual(deletedProp?.CurrentValue, prop.CurrentValue))
                {
                    yield return CreateAuditChange(deletedValueObject, propertyName, prop, originalValue: deletedProp?.OriginalValue, currentValue: prop.CurrentValue);
                }
            }
        }
    }

    private static ChangeTrackerEvent GetDeepChangesInline<TDomainEntity, TDbContext>(
        TDbContext dbContext,
        EntityEntry<TDomainEntity> entry,
        ChangeTrackerEvent? parent)
        where TDomainEntity : DomainEntity
        where TDbContext : DbContext
    {
        var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        var parentPrimaryKey = parent?.EntityId;

        // Consider only properties that are NOT temporary and add the primary key ONLY if it
        var props = entry.Properties
            .Where(p =>
                !p.IsTemporary &&
                !p.Metadata.IsPrimaryKey() && // Ignore child primary Key
                !(p.Metadata.IsForeignKey() && p.Metadata.GetPrincipals().Any(pr => pr.DeclaringEntityType.Name == parent?.EntityTypeFullName) && (p.CurrentValue?.ToString()?.Equals(parentPrimaryKey) ?? false))); // Ignore child foreign key to the parent

        var changeState = entry.State;

        // Change the state to Deleted if the entity is ISoftDeletable and it's being deleted
        if (entry.Entity is ISoftDeletable
            && props.Any(p => p.Metadata.Name == nameof(ISoftDeletable.IsDeleted)
            && Convert.ToBoolean(p.CurrentValue) == true))
        {
            changeState = EntityState.Deleted;
        }

        var entityType = entry.Entity.GetType();

        var currentEntityAuditEvent = new ChangeTrackerEvent(
               entry.Entity,
               primaryKey!.CurrentValue!,
               entityType,
               _entityStateToAuditLogChangeTypeMapping[changeState]);

        switch (changeState)
        {
            case EntityState.Added:
                foreach (var prop in props)
                {
                    if (!ShouldIgnoreProp(entityType, prop) &&
                        prop.Metadata.Name != nameof(ISoftDeletable.IsDeleted)) // Ignore changes from NULL to false for the IsDeleted flag
                    {
                        currentEntityAuditEvent.AddChanges(CreateAuditChange(entry, null, prop, originalValue: null, prop.CurrentValue));
                    }
                }
                break;
            case EntityState.Deleted:
                foreach (var prop in props)
                {
                    if (!ShouldIgnoreProp(entityType, prop))
                    {
                        if (prop.Metadata.Name == nameof(ISoftDeletable.IsDeleted))
                        {
                            currentEntityAuditEvent.AddChanges(CreateAuditChange(entry, null, prop, originalValue: false, currentValue: true));
                        }
                        else
                        {

                            currentEntityAuditEvent.AddChanges(CreateAuditChange(entry, null, prop, originalValue: prop.CurrentValue, currentValue: null));
                        }
                    }
                }
                break;
            default:
                props = props.Where(p => p.IsModified);

                foreach (var prop in props)
                {
                    if (!ShouldIgnoreProp(entityType, prop) &&
                        !ArePropertiesEqual(prop.OriginalValue, prop.CurrentValue))
                    {
                        currentEntityAuditEvent.AddChanges(CreateAuditChange(entry, null, prop, prop.OriginalValue, prop.CurrentValue));
                    }
                }
                break;
        }

        var changedCollectionsAndChildren = GetChangedCollectionsAndChildren(dbContext, entry);
        foreach (var collectionAndChildren in changedCollectionsAndChildren)
        {
            // collections & reference
            foreach (var childEntry in collectionAndChildren.Entities)
            {
                var child = GetDeepChangesInline(dbContext, childEntry, currentEntityAuditEvent);
                if (child != null && (child.Changes.Any() || child.Children.Any()))
                {
                    currentEntityAuditEvent.AddChild(child);
                }
            }

            foreach (var valueObjectEntry in collectionAndChildren.ValueObjects.GroupBy(v => v.Metadata.Name))
            {
                var changes = GetValueObjectChangesInline(collectionAndChildren.Name, valueObjectEntry);
                currentEntityAuditEvent.AddChanges(changes.ToArray());
            }

            //if (changesList.Any())
            //{
            //    changes.Add(collectionAndChildren.Key, changesList);
            //}
        }

        // Change the Entity Id to any transformed value only when finished doing all processing
        currentEntityAuditEvent
            .ChangeEntityId(GetValue(entry, primaryKey!.Metadata.Name, primaryKey!.CurrentValue!, primaryKey.Metadata.ClrType)!);

        return currentEntityAuditEvent;
    }

    private static IEnumerable<ChildTrackedChanges> GetChangedCollectionsAndChildren<TDomainEntity, TDbContext>(
        TDbContext dbContext, EntityEntry<TDomainEntity> entry)
        where TDomainEntity : DomainEntity
        where TDbContext : DbContext
    {
        var entryPk = entry.Properties.First(p => p.Metadata.IsPrimaryKey()).CurrentValue;
        foreach (var collection in entry.Collections.Where(c => c.CurrentValue != null))
        {
            var collectionEntityType = collection.CurrentValue!.GetType().GetGenericArguments().Single();
            var changedEntityEntries = dbContext.ChangeTracker
                .Entries<DomainEntity>()
                .Where(en => en.Metadata.ClrType == collectionEntityType &&
                    en.Properties.Any(p => p.Metadata.IsForeignKey() && ((p.OriginalValue?.Equals(entryPk) ?? false) || (p.CurrentValue?.Equals(entryPk) ?? false))) &&
                    (en.State.IsChanged() || GetChangedCollectionsAndChildren(dbContext, en).Any()));
            if (changedEntityEntries.Any())
            {
                yield return new ChildTrackedChanges(collection.Metadata.Name, changedEntityEntries.ToList(), new List<EntityEntry<ValueObject>>());
            }
        }

        foreach (var reference in entry.References.Where(c => c.CurrentValue != null))
        {
            var referenceEntityType = reference.CurrentValue!.GetType();

            if (referenceEntityType.IsAssignableTo(typeof(ValueObject)))
            {
                var valueObjectName = reference.Metadata.Name;

                if (reference.IsModified)
                {
                    var changedEntityEntries = dbContext.ChangeTracker
                        .Entries<ValueObject>()
                        .Where(en => en.Metadata.ClrType == referenceEntityType &&
                            en.Properties.Any(p => p.Metadata.IsForeignKey() && ((p.OriginalValue?.Equals(entryPk) ?? false) || (p.CurrentValue?.Equals(entryPk) ?? false))) &&
                            en.State.IsChanged() &&
                            en.Metadata.Name.Contains(valueObjectName));
                    if (changedEntityEntries.Any())
                    {
                        yield return new ChildTrackedChanges(reference.Metadata.Name, new List<EntityEntry<DomainEntity>>(), changedEntityEntries.ToList());
                    }
                }
            }
            else
            {
                var entryFk = reference.TargetEntry!.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())!.CurrentValue;
                var changedEntityEntries = dbContext.ChangeTracker
                    .Entries<DomainEntity>()
                    .Where(en => en.Metadata.ClrType == referenceEntityType &&
                        en.Properties.Any(p => p.Metadata.IsPrimaryKey() && ((p.OriginalValue?.Equals(entryFk) ?? false) || (p.CurrentValue?.Equals(entryFk) ?? false))) &&
                        (en.State.IsChanged() || GetChangedCollectionsAndChildren(dbContext, en).Any()) &&
                        !en.Entity.GetType().InheritsFromGenericType(typeof(AggregateRoot<>))); // Avoid to get changes for AggregateRoots, as they will be tracked separately!
                if (changedEntityEntries.Any())
                {
                    yield return new ChildTrackedChanges(reference.Metadata.Name, changedEntityEntries.ToList(), new List<EntityEntry<ValueObject>>());
                }
            }
        }
    }

    private static bool ArePropertiesEqual(object? oldValue, object? newValue)
    {
        if (oldValue == null && newValue == null)
        {
            return true;
        }

        if (oldValue != null && newValue == null)
        {
            return false;
        }

        if (oldValue == null && newValue != null)
        {
            return false;
        }

        return oldValue!.Equals(newValue!);
    }

    private static bool ShouldIgnoreProp(Type entityType, PropertyEntry? prop)
    {
        // NOTE: if PropertyInfo is NULL, it's a field and we can ignore it
        if (prop is null || prop.Metadata is null || prop.Metadata.PropertyInfo is null)
        {
            return true;
        }

        return _defaultIgnoredProperties.Contains(prop.Metadata.Name) ||
            AuditPropertyIgnoreHelper.IgnoreProperty(prop.Metadata.Name, entityType);
    }

    private static PropertyChange CreateAuditChange(
        EntityEntry entry,
        string? parentName,
        PropertyEntry prop,
        object? originalValue,
        object? currentValue)
    {
        var propertyName = AuditPropertyNameHelper.GetCustomPropertyName(prop.Metadata.Name, entry.Entity.GetType());
        propertyName = parentName != null ? parentName + "#" + propertyName : propertyName;

        return new PropertyChange(
            propertyName,
            prop.Metadata.ClrType,
            GetValue(entry, prop.Metadata.Name, originalValue, prop.Metadata.ClrType),
            GetValue(entry, prop.Metadata.Name, currentValue, prop.Metadata.ClrType));
    }

    private static object? GetValue(
        EntityEntry entry,
        string propertyName,
        object? propertyValue,
        Type propertyType)
        => AuditPropertyValueHelper.GetValueOrOverride(entry, propertyName, propertyType, propertyValue);


    private record ChildTrackedChanges
    {
        public string Name { get; }
        public IEnumerable<EntityEntry<DomainEntity>> Entities { get; }
        public IEnumerable<EntityEntry<ValueObject>> ValueObjects { get; }

        public ChildTrackedChanges(
            string name,
            IEnumerable<EntityEntry<DomainEntity>>? entities,
            IEnumerable<EntityEntry<ValueObject>>? valueObjects)
        {
            Name = name;
            Entities = entities ?? new List<EntityEntry<DomainEntity>>();
            ValueObjects = valueObjects ?? new List<EntityEntry<ValueObject>>();
        }
    }
}
