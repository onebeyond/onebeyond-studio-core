using System;
using Azure.Data.Tables;
using EnsureThat;
using Newtonsoft.Json.Linq;
using OneBeyond.Studio.Crosscuts.DateTimes;
using OneBeyond.Studio.EntityAuditing.AzureTableStorage.Util;
using OneBeyond.Studio.EntityAuditing.Domain;

namespace OneBeyond.Studio.EntityAuditing.AzureTableStorage.Entities;

public static class TableEntityHelper
{
    private const string JSON_CHANGES_COLUMN = "ChangedData";
    private const string OLD_PREFIX = "Old";
    private const string NEW_PREFIX = "New";

    public static TableEntity CreateEntity(AuditEvent auditEntityEvent, JToken changes, bool expandValuesInTableColumns)
    {
        EnsureArg.IsNotNull(auditEntityEvent, nameof(auditEntityEvent));

        // Need to turn the identity into a string key (if is an int, pad to 10 digits)
        var partitionKey = StorageTableUtils.BuildPartitionKey(auditEntityEvent.EntityId);

        // As this is an audit, we want to use the log-tail pattern
        // in order to optimize TOP queries looking for most recent changes
        // Ref: https://docs.microsoft.com/en-gb/azure/cosmos-db/table-storage-design-guide#log-tail-pattern
        var rowKey = DateTime.UtcNow.ToInvertedTicks();

        var tableEntity = new TableEntity(partitionKey, rowKey)
        {
            ["UserId"] = auditEntityEvent.UserId,
            ["EventType"] = auditEntityEvent.EventType
        };

        if (auditEntityEvent.EntityDescription != null)
        {
            tableEntity["EntityDescriptor"] = auditEntityEvent.EntityDescription;
        }

        if (expandValuesInTableColumns)
        {
            foreach (var change in changes)
            {
                var propertyName = change[nameof(AuditChange.PropertyName)];
                var oldValue = change[nameof(AuditChange.OriginalValue)];
                var newValue = change[nameof(AuditChange.NewValue)];
                tableEntity[$"{OLD_PREFIX}_{propertyName}"] = oldValue.ToEntityProperty();
                tableEntity[$"{NEW_PREFIX}_{propertyName}"] = newValue.ToEntityProperty();
            }
        }

        tableEntity[JSON_CHANGES_COLUMN] = changes.ToString();

        return tableEntity;
    }
}
