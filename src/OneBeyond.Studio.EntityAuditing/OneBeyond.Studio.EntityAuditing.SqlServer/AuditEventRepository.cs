using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MoreLinq;
using OneBeyond.Studio.Crosscuts.Strings;
using OneBeyond.Studio.EntityAuditing.SqlServer.Entities;

namespace OneBeyond.Studio.EntityAuditing.SqlServer;

internal sealed class AuditEventRepository : IAuditEventRepository
{
    private sealed class PropertyMapping
    {
        internal PropertyMapping(
            string propertyName,
            IProperty dbProperty)
        {
            EnsureArg.IsNotNullOrWhiteSpace(propertyName, nameof(propertyName));
            EnsureArg.IsNotNull(dbProperty, nameof(dbProperty));

            var mappedDbColumn = dbProperty.GetTableColumnMappings().First()!;

            PropertyName = propertyName;
            DataType = mappedDbColumn.Column.ProviderClrType.FullName!;
            ColumnName = mappedDbColumn.Column.Name;
            IsNullable = mappedDbColumn.Column.IsNullable;
        }

        public string PropertyName { get; }
        public string ColumnName { get; }
        public string DataType { get; }
        public bool IsNullable { get; }
    }

    private readonly DbContext _dbContext;
    private readonly Lazy<DbSet<AuditEvent>> _dbSet;
    private readonly List<PropertyMapping> _typeMappings = new(); //Used for bulk inserts

    public AuditEventRepository(AuditDbContext dbContext)
    {
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
        _dbSet = new Lazy<DbSet<AuditEvent>>(() => _dbContext.Set<AuditEvent>());

        _typeMappings = CreateTypeMappings(
            typeof(AuditEvent),
            _dbContext.Model.FindEntityType(typeof(AuditEvent))!.GetProperties());
    }

    public Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNull(auditEvent, nameof(auditEvent));

        _dbSet.Value.Add(auditEvent);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddBulkAsync(IReadOnlyCollection<AuditEvent> auditEvents, CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNull(auditEvents, nameof(auditEvents));

        var dataTable = CreateBulkInsertDataTable();

        dataTable = AddDataTableRecords(auditEvents, dataTable);

        await PerformBulkInsertAsync(dataTable, cancellationToken);
    }

    private static List<PropertyMapping> CreateTypeMappings(
            Type? type,
            IEnumerable<IProperty> dbProperties)
    {
        var properties = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(prop => prop.CanWrite)
            .ToList();

        return properties.Select(prop => 
            new PropertyMapping(
                prop.Name,
                dbProperties.FirstOrDefault(p => p.Name == prop.Name)))
            .ToList();
    }

    private DataTable CreateBulkInsertDataTable()
    {
        var dataTable = new DataTable();

        _typeMappings
            .ForEach((column) =>
            {
                var col = dataTable.Columns.Add(column.ColumnName);

                if (!column.DataType.IsNullOrWhiteSpace())
                {
                    col.DataType = Type.GetType(column.DataType);
                }
                col.AllowDBNull = column.IsNullable;
            });
        return dataTable;
    }

    private DataTable AddDataTableRecords(IEnumerable<AuditEvent> entities, DataTable dataTable)
    {
        var entityType = entities.First().GetType();

        entities.ForEach((entity) =>
        {
            var row = dataTable.NewRow();
            _typeMappings
                .ForEach((column) =>
                {
                    row[column.ColumnName] = GetPropertyValue(entityType, entity, column.PropertyName);
                });

            dataTable.Rows.Add(row);
        });

        return dataTable;
    }

    private async Task PerformBulkInsertAsync(
        DataTable dataTable,
        CancellationToken cancellationToken)
    {
        using var sqlBulkCopy = new SqlBulkCopy(_dbContext.Database.GetConnectionString());

        sqlBulkCopy.DestinationTableName = _dbContext.Model.FindEntityType(typeof(AuditEvent))!.GetSchemaQualifiedTableName()!;

        _typeMappings
            .ForEach((column) =>
            {
                sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            });

        await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);
    }

    private static object GetPropertyValue(Type entityType, object entity, string propertyName)
    {
        if (entity is null)
        {
            return DBNull.Value;
        }

        var property = entityType.GetProperty(propertyName)!;

        return property.GetValue(entity) ?? DBNull.Value;
    }

}
