using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.EntityAuditing.Domain;

namespace OneBeyond.Studio.EntityAuditing.SqlServer;

public class AuditDbWriter<TEntity> : IAuditWriter<TEntity>
    where TEntity : class
{
    private readonly AuditDbConverter<TEntity> _converter;
    private readonly IAuditEventRepository _repository;

    public AuditDbWriter(
        AuditDbConverter<TEntity> converter,
        IAuditEventRepository repository)
    {
        EnsureArg.IsNotNull(converter);
        EnsureArg.IsNotNull(repository);

        _converter = converter;
        _repository = repository;
    }

    public virtual async Task WriteAsync(TEntity entity, AuditEvent auditEntityEvent, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(entity, nameof(entity));
        EnsureArg.IsNotNull(auditEntityEvent, nameof(auditEntityEvent));

        var auditEvent = await _converter.ConvertAsync(entity, auditEntityEvent, cancellationToken);
        if (auditEvent is not null)
        {
            await _repository.AddAsync(auditEvent, cancellationToken);
        }
    }
}

