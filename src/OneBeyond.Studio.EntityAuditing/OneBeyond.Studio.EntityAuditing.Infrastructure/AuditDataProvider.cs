using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audit.EntityFramework;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.DomainEvents;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.EntityAuditing.Domain;
using OneBeyond.Studio.EntityAuditing.Infrastructure.Attributes;
using OneBeyond.Studio.EntityAuditing.Infrastructure.Options;

namespace OneBeyond.Studio.EntityAuditing.Infrastructure;

public sealed class AuditDataProvider : Audit.Core.AuditDataProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EntityAuditingOptions _options;

    public AuditDataProvider(IServiceProvider serviceProvider)
    {
        EnsureArg.IsNotNull(serviceProvider);

        _serviceProvider = serviceProvider;
        // NOTE: cannot use "GetRequiredService" as it would crash during migrations (unable to create DbContext)
        _options = serviceProvider.GetService<EntityAuditingOptions>();
    }

    public override object InsertEvent(Audit.Core.AuditEvent auditEvent)
        => throw new NotSupportedException();

    public override Task<object> InsertEventAsync(Audit.Core.AuditEvent auditEvent, CancellationToken cancellationToken = default)
        => ProcessEventAsync(auditEvent);

    private async Task<object> ProcessEventAsync(Audit.Core.AuditEvent auditEvent)
    {
        if (auditEvent is not AuditEventEntityFramework efAudit)
        {
            // Don't audit non EF audits currently.
            return 0;
        }

        if (!efAudit.EntityFrameworkEvent.Success)
        {
            // Do not create an audit if SaveChanges was unsuccessful
            return 0;
        }

        var entries = efAudit.EntityFrameworkEvent.Entries
            .Where((entry) => entry.Valid) // Do not track invalid objects
            .Where((entry) => entry.Entity != null)
            .Where((entry) => !entry.Entity.GetType().IsGenericTypeDefinition) // avoid many-to-many db changes (dictionary)
            .Where((entry) => entry.Entity is not RaisedDomainEvent) // ignore domain events
            .Where((entry) => IsEntityTracked(entry)); // Respect attributes and mode

        // Split out individual entities from event to track
        foreach (var entry in entries)
        {
            // Force deletion action if IsDeleted flag has been set to true
            HandleISoftDeletable(entry);

            // Ignore direct changes on ValueObjects as they are tracked through their parents
            // NOTE: they must still be included in the entries collection, as they are needed to find changes through related parents
            if (IsValueObject(entry.Entity.GetType()))
            {
                continue;
            }

            var auditContext = _serviceProvider.GetRequiredService<IAuditContext>();
            var auditEventEntity = new AuditEvent
            {
                EventType = entry.Action,
                InsertedDate = DateTime.UtcNow,
                EntityId = string.Join(":", entry.PrimaryKey.Values),
                EntityDescription = AuditAttributesHelper.GetPropertyDescriptorValue(entry)?.ToString(),
                UserId = auditContext?.AuthorId ?? Guid.Empty.ToString()
            };

            GetChangesFromNavigationProperty(entries, entry, auditEventEntity);

            auditEventEntity.AddChanges(GetActualChanges(entry));
            if (!auditEventEntity.EntityChanges.Any() && !auditEventEntity.NavigationPropertyAuditEvents.Any())
            {
                continue;
            }

            var entityType = entry.Entity.GetType();
            var auditWriterType = typeof(IAuditWriter<>).GetGenericTypeDefinition().MakeGenericType(entityType);

            var auditWriter = (IAuditWriter)_serviceProvider.GetService(auditWriterType);
            if (auditWriter is not null)
            {
                var writeAsyncFunc = AuditWriterFactory.GetOrCompileWriteFunction(entityType);
                await writeAsyncFunc(auditWriter, entry.Entity, auditEventEntity);
            }
        }

        return 1;
    }

    /// <summary>
    /// // Force delete operation if the entity is ISoftDeletable and IsDeleted = true
    /// </summary>
    /// <param name="entry"></param>
    private static void HandleISoftDeletable(EventEntry entry)
    {
        if (entry.Action == AuditActionType.Update.Name
            && entry.Entity is ISoftDeletable
            && entry.Changes.Any(c =>
                c.ColumnName == nameof(ISoftDeletable.IsDeleted) && Convert.ToBoolean(c.NewValue)))
        {

            entry.Action = AuditActionType.Delete.Name;
        }
    }

    /// <summary>
    /// Find any child navigation property and attach the related changes (if any) to the parent entity
    /// </summary>
    /// <param name="allChanges"></param>
    /// <param name="currentEntry"></param>
    /// <param name="auditEventEntity"></param>
    private void GetChangesFromNavigationProperty(
        IEnumerable<EventEntry> allChanges,
        EventEntry currentEntry,
        AuditEvent auditEventEntity)
    {
        // Get all navigation properties that are collections
        foreach (var navigation in currentEntry.GetEntry().Collections)
        {
            AddCollectionNavigationChanges(allChanges, currentEntry, auditEventEntity, navigation);
        }

        // Get all navigation properties that are NOT collections
        foreach (var navigation in currentEntry.GetEntry().References)
        {
            AddReferenceNavigationChanges(allChanges, auditEventEntity, navigation);
        }
    }

    private void AddReferenceNavigationChanges(
        IEnumerable<EventEntry> allChanges,
        AuditEvent auditEventEntity,
        ReferenceEntry navigation)
    {
        var navigationType = navigation.Metadata.ClrType;
        var navigationName = navigation.Metadata.PropertyInfo.Name;
        var isValueObject = IsValueObject(navigationType);

        if (isValueObject)
        {
            AddValueObjectAuditEvent(allChanges, auditEventEntity, navigation, navigationType, navigationName);
        }
        else
        {
            // Find any navigation property that changed within the same transaction
            // Only "Insert" or "Update" are detected here. But, anyway, a "Delete"
            // for a reference navigation means just that the child entity is detached
            // from its parent (thus, it's not a real deletion and it's detected through
            // direct changes, as the foreign key(s) will become null)
            var navigationChanges = allChanges
                .Where(c => c.Entity.GetType() == navigationType && c.Entity.Equals(navigation.CurrentValue));

            foreach (var navigationChange in navigationChanges)
            {
                AddNavigationPropertyAuditEvent(auditEventEntity, navigationName, navigationChange);
            }
        }
    }

    private void AddCollectionNavigationChanges(
        IEnumerable<EventEntry> allChanges,
        EventEntry currentEntry,
        AuditEvent auditEventEntity,
        CollectionEntry navigation)
    {
        var currentEntityType = currentEntry.Entity.GetType();

        var compositeForeignKeys = new Dictionary<string, object>();
        IForeignKey foreignKey = null;

        if (navigation.Metadata is INavigation navigationMetadata)
        {
            foreignKey = navigationMetadata.ForeignKey;
        }
        else if (navigation.Metadata is ISkipNavigation skipNavigationMetadata)
        {
            foreignKey = skipNavigationMetadata.ForeignKey;
        }

        if (foreignKey == null)
        {
            return;
        }

        var navigationEntity = foreignKey.DeclaringEntityType;
        var navigationName = navigation.Metadata.PropertyInfo.Name;
        var navigationType = navigationEntity.ClrType;

        for (var i = 0; i < foreignKey.Properties.Count; i++)
        {
            var foreignKeyName = foreignKey.Properties[i].Name;
            var principalKeyName = foreignKey.PrincipalKey.Properties[i].Name;
            var keyValue = currentEntry.GetEntry().CurrentValues[principalKeyName];
            compositeForeignKeys[foreignKeyName] = keyValue;
        }

        if (compositeForeignKeys.Any())
        {
            var navigationChanges = allChanges
                .Where(c => c.Entity.GetType() == navigationType &&
                            c.Entity.GetType() != currentEntityType &&
                            compositeForeignKeys.All(x => c.ColumnValues.ContainsKey(x.Key) && c.ColumnValues[x.Key].Equals(x.Value)));

            foreach (var navigationChange in navigationChanges)
            {
                AddNavigationPropertyAuditEvent(auditEventEntity, navigationName, navigationChange);
            }
        }

        // Get a second level of navigation properties(useful for many - to - many or one - to - many relationships)
        // Only "Insert" or "Update" are hereby detected
        foreach (var subNavigation in navigationEntity.GetNavigations().Where(n => n.ClrType != currentEntityType))
        {
            AddManyToManyNavigationChanges(allChanges, auditEventEntity, navigation, navigationName, navigationType, subNavigation);
        }
    }

    private void AddManyToManyNavigationChanges(
        IEnumerable<EventEntry> allChanges,
        AuditEvent auditEventEntity,
        CollectionEntry navigation,
        string navigationName,
        Type navigationType,
        INavigation subNavigation)
    {
        var foreignKeyAndPrincipalKeyNames = new Dictionary<string, string>();
        for (var i = 0; i < subNavigation.ForeignKey.Properties.Count; i++)
        {
            var foreignKeyName = subNavigation.ForeignKey.Properties[i].Name;
            var principalKeyName = subNavigation.ForeignKey.PrincipalKey.Properties[i].Name;
            foreignKeyAndPrincipalKeyNames[foreignKeyName] = principalKeyName;
        }

        var subNavigationChanges = new List<EventEntry>();

        foreach (var value in navigation.CurrentValue)
        {
            var primaryKeyValues = new Dictionary<string, object>();
            foreach (var fk in foreignKeyAndPrincipalKeyNames)
            {
                var property = value.GetType().GetProperty(fk.Key);
                if (property != null)
                {
                    var navigationIdValue = property.GetValue(value);
                    primaryKeyValues[fk.Value] = navigationIdValue;
                }
            }

            subNavigationChanges.AddRange(allChanges
                .Where(c => c.Entity.GetType() == subNavigation.ClrType &&
                            c.Entity.GetType() != navigationType &&
                            primaryKeyValues.All(x => c.ColumnValues.ContainsKey(x.Key) && c.ColumnValues[x.Key].Equals(x.Value))));
        }

        foreach (var subNavigationChange in subNavigationChanges)
        {
            AddNavigationPropertyAuditEvent(auditEventEntity, navigationName, subNavigationChange);
        }
    }

    private static void AddValueObjectAuditEvent(
        IEnumerable<EventEntry> allChanges,
        AuditEvent auditEventEntity,
        ReferenceEntry navigation,
        Type navigationType,
        string navigationName)
    {
        // Get the display name of the value object according to EF representation
        var valueObjectName = navigation.Metadata.TargetEntityType.DisplayName();

        // As ValueObjects are immutable, we extract any "Insert" or "Delete" change and we consider it as an "Update"
        var insertChange = allChanges
            .FirstOrDefault(c => c.Entity.GetType() == navigationType && valueObjectName == c.Name && c.Entity.Equals(navigation.CurrentValue));
        var insertValue = insertChange?.Entity as ValueObject;

        var deleteChange = allChanges
            .FirstOrDefault(c => c.Entity.GetType() == navigationType && valueObjectName == c.Name && c.Action == AuditActionType.Delete);
        var deleteValue = deleteChange?.Entity as ValueObject;

        if ((insertValue is { } || deleteValue is { }) && insertValue != deleteValue)
        {
            auditEventEntity.AddChange(new AuditChange()
            {
                NewValue = insertValue,
                OriginalValue = deleteValue,
                PropertyName = navigationName,
                PropertyType = insertValue?.GetType()?.ToString() ?? deleteValue?.GetType()?.ToString()
            });
        }
    }

    private void AddNavigationPropertyAuditEvent(
        AuditEvent auditEventEntity,
        string navigationName,
        EventEntry navigationChange)
    {
        var navigationPropertyAuditEvent = new AuditNavigationPropertyEvent()
        {
            EventType = navigationChange.Action,
            PropertyName = navigationName,
            EntityType = navigationChange.Entity.GetType(),
            EntityId = string.Join(":", navigationChange.PrimaryKey.Values)
        };

        navigationPropertyAuditEvent.AddChanges(GetActualChanges(navigationChange));

        // Avoid storing duplicates
        if (!auditEventEntity.NavigationPropertyAuditEvents
            .Any(npc => npc.EntityType == navigationPropertyAuditEvent.EntityType
                     && npc.EntityId == navigationPropertyAuditEvent.EntityId
                     && npc.EventType == navigationPropertyAuditEvent.EventType))
        {
            auditEventEntity.AddNavigationAuditEvent(navigationPropertyAuditEvent);
        }
    }

    /// <summary>
    /// Get all the changes according to the action performed
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    private List<AuditChange> GetActualChanges(EventEntry entry)
    {
        var actualChanges = new List<AuditChange>();

        switch (entry.Action)
        {
            case var _ when entry.Action == AuditActionType.Update:
                {
                    AddChangesForUpdatedEntry(entry, actualChanges);
                    break;
                }
            case var _ when entry.Action == AuditActionType.Insert:
                {
                    AddChangesForCreatedEntry(entry, actualChanges);
                    break;
                }

            case var _ when entry.Action == AuditActionType.Delete:
                {
                    AddChangesForDeletedEntry(entry, actualChanges);
                    break;
                }
        }

        return actualChanges;
    }

    private void AddChangesForUpdatedEntry(EventEntry entry, List<AuditChange> actualChanges)
    {
        if (entry.Changes is null)
        {
            return;
        }

        var changes = entry.Changes
            .Where((change) => change.ColumnName != nameof(ISoftDeletable.IsDeleted))
            .Where((change) => !AuditAttributesHelper.IgnoreProperty(entry, change.ColumnName));

        foreach (var change in changes)
        {
            var auditChange = CreateAuditChange(change, entry);

            if (ChangeCanBeAdded(auditChange))
            {
                actualChanges.Add(auditChange);
            }
        }
    }

    private bool ChangeCanBeAdded(AuditChange auditChange)
    {
        if (auditChange.OriginalValue == null || auditChange.NewValue == null)
        {
            return auditChange.OriginalValue != null || auditChange.NewValue != null;
        }
        else
        {
            return !auditChange.OriginalValue.Equals(auditChange.NewValue);
        }
    }

    private AuditChange CreateAuditChange(EventEntryChange change, EventEntry entry)
    {
        var auditChange = new AuditChange();

        var originalColumnName = change.ColumnName;
        auditChange.PropertyName = AuditAttributesHelper.GetCustomPropertyName(entry, originalColumnName, change.ColumnName);
        auditChange.OriginalValue = AuditAttributesHelper.GetValueOrOverride(_serviceProvider, entry, originalColumnName, change.OriginalValue);
        auditChange.NewValue = AuditAttributesHelper.GetValueOrOverride(_serviceProvider, entry, originalColumnName, change.NewValue);
        auditChange.PropertyType = auditChange.NewValue != null
            ? auditChange.NewValue.GetType().ToString()
            : auditChange.OriginalValue != null ? auditChange.OriginalValue.GetType().ToString() : "";

        return auditChange;
    }

    private void AddChangesForCreatedEntry(EventEntry entry, List<AuditChange> actualChanges)
    {
        var originalValues = entry.GetEntry().CurrentValues;
        foreach (var prop in originalValues.Properties)
        {
            var columnName = GetColumnName(prop, entry.Table, entry.Schema);

            if (columnName == nameof(ISoftDeletable.IsDeleted) || AuditAttributesHelper.IgnoreProperty(entry, columnName))
            {
                continue;
            }

            var newValue = originalValues[prop];

            if (newValue == null)
            {
                continue;
            }

            actualChanges.Add(new AuditChange()
            {
                PropertyName = AuditAttributesHelper.GetCustomPropertyName(entry, columnName, columnName),
                PropertyType = newValue.GetType().ToString(),
                OriginalValue = null,
                NewValue = AuditAttributesHelper.GetValueOrOverride(_serviceProvider, entry, columnName, newValue)
            });
        }
    }

    private void AddChangesForDeletedEntry(EventEntry entry, List<AuditChange> actualChanges)
    {
        var originalValues = entry.GetEntry().CurrentValues;
        foreach (var prop in originalValues.Properties)
        {
            var columnName = GetColumnName(prop, entry.Table, entry.Schema);

            if (columnName == nameof(ISoftDeletable.IsDeleted) || AuditAttributesHelper.IgnoreProperty(entry, columnName))
            {
                continue;
            }

            var oldValue = originalValues[prop];

            if (oldValue == null)
            {
                continue;
            }

            actualChanges.Add(new AuditChange()
            {
                PropertyName = columnName,
                PropertyType = oldValue.GetType().ToString(),
                OriginalValue = AuditAttributesHelper.GetValueOrOverride(_serviceProvider, entry, columnName, oldValue),
                NewValue = null
            });
        }
    }

    private bool IsEntityTracked(EventEntry entry)
    {
        return _options?.TrackingMode == TrackingMode.TrackAllButIgnored
            ? !AuditAttributesHelper.IgnoreEntity(entry) // include only entities without DontTrack attribute
            : AuditAttributesHelper.IncludeEntity(entry); // include only entities with Track attribute
    }

    private static string GetColumnName(IProperty prop, string table, string schema)
        => prop.GetColumnName(StoreObjectIdentifier.Table(table, schema));

    private static bool IsValueObject(Type type)
        => type.IsAssignableTo(typeof(ValueObject));
}
