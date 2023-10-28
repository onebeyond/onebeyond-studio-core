using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task WriteAsync(object entity, AuditEvent auditEntityEvent, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(entity, nameof(entity));
        EnsureArg.IsNotNull(auditEntityEvent, nameof(auditEntityEvent));

        var entityType = entity.GetType();
        var auditConverterType = typeof(AuditDbConverter<>).GetGenericTypeDefinition().MakeGenericType(entityType);

        var auditConverter = (IAuditDbConverter)_serviceProvider.GetService(auditConverterType);
        if (auditConverter is not null)
        {
            //var auditEvent = await (auditConverter as AuditDbConverter<TEntity>)!.ConvertAsync(entity, auditEntityEvent, cancellationToken);
            var converAsyncFunc = AuditDbConverterFactory.GetOrCompileConvertFunction(entityType);
            var auditEvent = await converAsyncFunc(auditConverter, entity, auditEntityEvent);

            if (auditEvent is not null)
            {
                _auditEvents.Add(auditEvent);
            }
        }
    }

    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        if (_auditEvents.Any())
        {
            await _repository.AddBulkAsync(_auditEvents, cancellationToken);
            _auditEvents.Clear();
        }
    }
}
