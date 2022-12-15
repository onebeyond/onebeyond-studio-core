using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Crosscuts.Reflection;
using OneBeyond.Studio.EntityAuditing.Domain;

namespace OneBeyond.Studio.EntityAuditing.Infrastructure;

internal sealed class AuditWriterFactory
{
    private static readonly MethodInfo _auditWriterWriteMethodInfoAsync =
        Reflector.MethodFrom(() => WriteEntityEventAsync<object>(default, default, default))
            .GetGenericMethodDefinition();

    private static readonly ConcurrentDictionary<Type, Func<IAuditWriter, object, AuditEvent, Task>> _auditWriterWriterFuncs
        = new();

    public static Func<IAuditWriter, object, AuditEvent, Task> GetOrCompileWriteFunction(Type entityType)
    {
        if (!_auditWriterWriterFuncs.TryGetValue(entityType, out var auditWriterWriteFunc))
        {
            auditWriterWriteFunc = CreateWriteFunc(entityType);
            _auditWriterWriterFuncs.AddOrUpdate(entityType, auditWriterWriteFunc, (type, function) => function);
        }
        return auditWriterWriteFunc;
    }

    private static Func<IAuditWriter, object, AuditEvent, Task> CreateWriteFunc(Type entityType)
    {
        var methodInfo = _auditWriterWriteMethodInfoAsync.MakeGenericMethod(entityType);

        var auditWriterParameter = Expression.Parameter(typeof(IAuditWriter), "auditWriter");
        var entityParameter = Expression.Parameter(typeof(object), "entity");
        var eventParameter = Expression.Parameter(typeof(AuditEvent), "event");

        var methodCall = Expression.Call(
            methodInfo,
            auditWriterParameter,
            entityParameter,
            eventParameter);

        var lambda = Expression.Lambda<Func<IAuditWriter, object, AuditEvent, Task>>(
            methodCall,
            auditWriterParameter,
            entityParameter,
            eventParameter);
        return lambda.Compile();
    }

    private static Task WriteEntityEventAsync<TEntity>(
        IAuditWriter auditWriter,
        object entity,
        AuditEvent @event)
        where TEntity : class
        => ((IAuditWriter<TEntity>)auditWriter).WriteAsync((TEntity)entity, @event, CancellationToken.None);
}
