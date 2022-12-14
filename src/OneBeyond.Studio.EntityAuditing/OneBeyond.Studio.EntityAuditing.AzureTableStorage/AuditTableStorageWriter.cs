using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using EnsureThat;
using OneBeyond.Studio.EntityAuditing.AzureTableStorage.Entities;
using OneBeyond.Studio.EntityAuditing.AzureTableStorage.Options;
using OneBeyond.Studio.EntityAuditing.AzureTableStorage.Util;
using OneBeyond.Studio.EntityAuditing.Domain;
using OneBeyond.Studio.EntityAuditing.Domain.AuditEntityNameResolvers;
using OneBeyond.Studio.EntityAuditing.Domain.AuditEventSerializers;

namespace OneBeyond.Studio.EntityAuditing.AzureTableStorage;

public class AuditTableStorageWriter<TEntity> : IAuditWriter<TEntity>
    where TEntity : class
{
    private static readonly IDictionary<string, TableClient> TableClients = new ConcurrentDictionary<string, TableClient>();

    private readonly TableServiceClient _serviceClient;
    private readonly string _tablePrefix;
    private readonly bool _expandValuesInTableColumns;
    private readonly IAuditNameResolver<TEntity> _auditEntityTypeBuilder;
    private readonly IJsonAuditEventSerializer<TEntity> _auditEntityEventSerializer;

    public AuditTableStorageWriter(
        AzureTableStorageAuditingOptions options,
        IAuditNameResolver<TEntity> auditEntityTypeBuilder,
        IJsonAuditEventSerializer<TEntity> auditEntityEventSerializer)
    {
        EnsureArg.IsNotNull(options, nameof(options));
        EnsureArg.IsNotNull(auditEntityTypeBuilder);
        EnsureArg.IsNotNull(auditEntityEventSerializer);

        _serviceClient = new TableServiceClient(options.ConnectionString);
        _tablePrefix = string.IsNullOrWhiteSpace(options.TablePrefix) ? "audit" : options.TablePrefix.ToLowerInvariant();
        _expandValuesInTableColumns = options.ExpandValuesInTableColumns;
        _auditEntityTypeBuilder = auditEntityTypeBuilder;
        _auditEntityEventSerializer = auditEntityEventSerializer;
    }

    public virtual async Task WriteAsync(TEntity entity, AuditEvent auditEntityEvent, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(entity, nameof(entity));
        EnsureArg.IsNotNull(auditEntityEvent, nameof(auditEntityEvent));

        var entityName = _auditEntityTypeBuilder.GetEntityName();
        var changes = await _auditEntityEventSerializer.SerializeAsync(entity, auditEntityEvent, cancellationToken);

        if (auditEntityEvent.EventType == AuditActionType.Update.Name && changes.IsEmpty)
        {
            // Do not write anything if there are no changes to record
            return;
        }

        var dynamicTableEntity = TableEntityHelper.CreateEntity(auditEntityEvent, changes.Data, _expandValuesInTableColumns);
        var tableClient = await GetTableClientAsync(entityName, cancellationToken);
        await tableClient.AddEntityAsync(dynamicTableEntity, cancellationToken);
    }

    private async Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken)
    {
        tableName = StorageTableUtils.SanitiseTableName($"{_tablePrefix}{tableName}".ToLowerInvariant());

        if (!StorageTableUtils.IsValidTableName(tableName))
        {
            throw new ArgumentException("Invalid entity name: table names can only " +
                "contain alphanumeric characters, they cannot start with a number and they must be from 3 to 63 characters long");
        }

        if (TableClients.TryGetValue(tableName, out var cachedTableClient))
        {
            return cachedTableClient;
        }

        await _serviceClient.CreateTableIfNotExistsAsync(tableName, cancellationToken);
        var tableClient = _serviceClient.GetTableClient(tableName);
        TableClients[tableName] = tableClient;
        return tableClient;
    }
}
