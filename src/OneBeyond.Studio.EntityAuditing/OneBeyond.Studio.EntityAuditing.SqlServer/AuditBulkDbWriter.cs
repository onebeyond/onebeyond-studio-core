using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.EntityAuditing.Domain;

namespace OneBeyond.Studio.EntityAuditing.SqlServer;

public class AuditBulkDbWriter : IAuditBulkWriter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuditEventRepository _repository;

    private readonly List<Entities.AuditEvent> _auditEvents = new();

    public AuditBulkDbWriter(
        IServiceProvider serviceProvider,
        IAuditEventRepository repository)
    {
        EnsureArg.IsNotNull(serviceProvider);
        EnsureArg.IsNotNull(repository);

        _serviceProvider = serviceProvider;
        _repository = repository;
    }

    public async Task WriteAsync<TEntity>(TEntity entity, AuditEvent auditEntityEvent, CancellationToken cancellationToken)
        where TEntity : class
    {
        EnsureArg.IsNotNull(entity, nameof(entity));
        EnsureArg.IsNotNull(auditEntityEvent, nameof(auditEntityEvent));

        var entityType = entity.GetType();
        var auditConverterType = typeof(AuditDbConverter<>).GetGenericTypeDefinition().MakeGenericType(entityType);

        var auditConverter = _serviceProvider.GetService(auditConverterType);
        if (auditConverter is not null)
        {
            var auditEvent = await (auditConverter as AuditDbConverter<TEntity>)!.ConvertAsync(entity, auditEntityEvent, cancellationToken);
            _auditEvents.Add(auditEvent);
        }
    }

    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        await _repository.AddBulkAsync(_auditEvents, cancellationToken);
        _auditEvents.Clear();
    }
}
