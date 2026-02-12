using System.Linq.Expressions;
using System.Reflection;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using OneBeyond.Studio.Crosscuts.Reflection;

namespace OneBeyond.Studio.DataAccess.EFCore.Projections;

internal class EntityTypeProjections<TEntity> : IEntityTypeProjections<TEntity>
    where TEntity : class
{
    private static readonly MethodInfo DoProjectMethodInfo = Reflector
        .MethodFrom(() => DoProject<object, object>(default!, default!, default!))
        .GetGenericMethodDefinition();

    private readonly IReadOnlyDictionary<(Type EntityType, Type ResultType), DoProjectFunc> _doProjectFuncMap;

    public EntityTypeProjections(
        IEnumerable<IEntityTypeProjection> entityTypeProjections)
    {
        EnsureArg.IsNotNull(entityTypeProjections, nameof(entityTypeProjections));

        _doProjectFuncMap = entityTypeProjections
            .SelectMany(CreateDoProjectFuncMap)
            .Where(item => item.EntityType.IsAssignableFrom(typeof(TEntity)))
            .ToDictionary(item => (item.EntityType, item.ResultType), item => item.DoProject);
    }

    public IQueryable<TResult> ProjectTo<TResult>(IQueryable<TEntity> entityQuery, DbContext dbContext)
    {
        EnsureArg.IsNotNull(entityQuery, nameof(entityQuery));
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));

        var projectionContext = new ProjectionContext(dbContext);
        var resultType = typeof(TResult);

        var entityType = typeof(TEntity);
        while (entityType is not null)
        {
            var key = (entityType, resultType);
            if (_doProjectFuncMap.TryGetValue(key, out var doProject))
            {
                return (IQueryable<TResult>)doProject(entityQuery, projectionContext);
            }
            entityType = entityType.BaseType;
        }

        throw new InvalidOperationException($"No projection specified from '{typeof(TEntity).FullName}' (or any of its base types) to '{typeof(TResult).FullName}'.");
    }

    private static IReadOnlyCollection<(Type EntityType, Type ResultType, DoProjectFunc DoProject)> CreateDoProjectFuncMap(
        IEntityTypeProjection entityTypeProjection)
    {
        var projectionType = entityTypeProjection.GetType();
        var doProjectFuncMap = projectionType.GetInterfaces()
            .Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEntityTypeProjection<,>))
            .Select(interfaceType =>
            {
                var typeArguments = interfaceType.GetGenericArguments();
                var entityType = typeArguments[0];
                var resultType = typeArguments[1];
                return (entityType, resultType, CompileDoProjectFunc(entityTypeProjection, entityType, resultType));
            })
            .ToList();

        return doProjectFuncMap.Count == 0
            ? throw new ArgumentOutOfRangeException(
                nameof(entityTypeProjection),
                $"Entity type projection of the {projectionType.FullName} type is incomplete. Consider implementing at least one projection.")
            : doProjectFuncMap;
    }

    private static DoProjectFunc CompileDoProjectFunc(
        IEntityTypeProjection entityTypeProjection,
        Type entityType,
        Type resultType)
    {
        var interfaceType = typeof(IEntityTypeProjection<,>).MakeGenericType(entityType, resultType);
        var projectionInstance = Expression.Constant(entityTypeProjection, interfaceType);
        var entityQueryParam = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(entityType), "entityQuery");
        var projectionContextParam = Expression.Parameter(typeof(ProjectionContext), "context");

        var doProjectCall = Expression.Call(
            DoProjectMethodInfo.MakeGenericMethod(entityType, resultType),
            projectionInstance,
            entityQueryParam,
            projectionContextParam);

        var doProjectLambda = Expression.Lambda<DoProjectFunc>(
            doProjectCall,
            entityQueryParam,
            projectionContextParam);

        return doProjectLambda.Compile();
    }

    private static IQueryable<TResult> DoProject<TSource, TResult>(
        IEntityTypeProjection<TSource, TResult> entityTypeProjection,
        IQueryable<TSource> entityQuery,
        ProjectionContext context)
        where TSource : class
        => entityTypeProjection.Project(entityQuery, context);

    private delegate object DoProjectFunc(IQueryable entityQuery, ProjectionContext context);
}
