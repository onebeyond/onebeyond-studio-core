using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Crosscuts.Reflection;
using OneBeyond.Studio.EntityAuditing.Domain;

namespace OneBeyond.Studio.EntityAuditing.SqlServer;

internal static class AuditDbConverterFactory
{
    private static readonly MethodInfo _auditConverterConvertMethodInfoAsync =
        Reflector.MethodFrom(() => ConvertEntityEventAsync<object>(default, default, default))
            .GetGenericMethodDefinition();

    private static readonly ConcurrentDictionary<Type, Func<IAuditDbConverter, object, AuditEvent, Task<Entities.AuditEvent>>> _auditConverterFuncs
        = new();

    public static Func<IAuditDbConverter, object, AuditEvent, Task<Entities.AuditEvent>> GetOrCompileConvertFunction(Type entityType)
    {
        if (!_auditConverterFuncs.TryGetValue(entityType, out var auditConverterFunc))
        {
            auditConverterFunc = CreateConvertFunc(entityType);
            _auditConverterFuncs.AddOrUpdate(entityType, auditConverterFunc, (type, function) => function);
        }
        return auditConverterFunc;
    }

    private static Func<IAuditDbConverter, object, AuditEvent, Task<Entities.AuditEvent>> CreateConvertFunc(Type entityType)
    {
        var methodInfo = _auditConverterConvertMethodInfoAsync.MakeGenericMethod(entityType);

        var auditWriterParameter = Expression.Parameter(typeof(IAuditDbConverter), "auditConverter");
        var entityParameter = Expression.Parameter(typeof(object), "entity");
        var eventParameter = Expression.Parameter(typeof(AuditEvent), "event");

        var methodCall = Expression.Call(
            methodInfo,
            auditWriterParameter,
            entityParameter,
            eventParameter);

        var lambda = Expression.Lambda<Func<IAuditDbConverter, object, AuditEvent, Task<Entities.AuditEvent>>>(
            methodCall,
            auditWriterParameter,
            entityParameter,
            eventParameter);

        return lambda.Compile();
    }

    private static Task ConvertEntityEventAsync<TEntity>(
        IAuditDbConverter auditConverter,
        object entity,
        AuditEvent @event)
        where TEntity : class
        => ((AuditDbConverter<TEntity>)auditConverter).ConvertAsync((TEntity)entity, @event, CancellationToken.None);
}

