using System.Threading.Tasks;
using OneBeyond.Studio.EntityAuditing.Domain;

namespace OneBeyond.Studio.EntityAuditing.Infrastructure;

internal static class AuditWriterExtensions
{
    internal static Task WriteAsync<T>(this IAuditWriter<T> auditWriter, T entity, AuditEvent auditEntityEvent)
        where T : class
        => auditWriter.WriteAsync(entity, auditEntityEvent);
}
