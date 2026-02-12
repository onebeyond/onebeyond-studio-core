using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using OneBeyond.Studio.Crosscuts.Reflection;

namespace OneBeyond.Studio.DataAccess.EFCore.Projections;

/// <summary>
/// Provides an abstract base class for managing and optimizing entity type projections.
/// </summary>
/// <remarks>This class supports the creation and caching of projection delegates for entity types.</remarks>
internal abstract class EntityTypeProjections
{
    private static readonly ConditionalWeakTable<IEntityTypeProjection, ConcurrentDictionary<(Type EntityType, Type ResultType), ProjectFunc>> _delegates = [];

    protected delegate object ProjectFunc(IQueryable entityQuery, ProjectionContext context);

    protected static Type ProjectionInterfaceType { get; } = typeof(IEntityTypeProjection<,>);
    protected static Type QueryableType { get; } = typeof(IQueryable<>);

    protected static MethodInfo ProjectMethodInfo { get; } = Reflector
        .MethodFrom(() => Project<object, object>(default!, default!, default!))
        .GetGenericMethodDefinition();

    protected static IQueryable<TResult> Project<TSource, TResult>(
        IEntityTypeProjection<TSource, TResult> entityTypeProjection,
        IQueryable<TSource> entityQuery,
        ProjectionContext context)
        where TSource : class
        => entityTypeProjection.Project(entityQuery, context);

    protected static ProjectFunc GetOrCompileProjectFunc(
        IEntityTypeProjection entityTypeProjection,
        Type entityType,
        Type resultType)
    {
        var instanceCache = _delegates.GetOrCreateValue(entityTypeProjection);

        var key = (entityType, resultType);

        return instanceCache.GetOrAdd(key, _ =>
        {
            var interfaceType = ProjectionInterfaceType.MakeGenericType(entityType, resultType);
            var projectionInstance = Expression.Constant(entityTypeProjection, interfaceType);

            var entityQueryParam = Expression.Parameter(typeof(IQueryable), "entityQuery");
            var castedEntityQuery = Expression.Convert(entityQueryParam, QueryableType.MakeGenericType(entityType));
            var projectionContextParam = Expression.Parameter(typeof(ProjectionContext), "context");

            var projectCall = Expression.Call(
                ProjectMethodInfo.MakeGenericMethod(entityType, resultType),
                projectionInstance,
                castedEntityQuery,
                projectionContextParam);

            var projectLambda = Expression.Lambda<ProjectFunc>(
                projectCall,
                entityQueryParam,
                projectionContextParam);

            return projectLambda.Compile();
        });
    }
}

/// <summary>
/// Provides functionality to project entities of type <typeparamref name="TEntity"/> into different result types based on registered projections.
/// </summary>
/// <remarks>It builds a hierarchy of entity types to support inheritance-based projections.</remarks>
internal sealed class EntityTypeProjections<TEntity> : EntityTypeProjections, IEntityTypeProjections<TEntity>
    where TEntity : class
{
    private readonly FrozenDictionary<(Type EntityType, Type ResultType), ProjectFunc> _projectFuncMap;
    private readonly IReadOnlyList<Type> _entityTypeHierarchy;

    public EntityTypeProjections(
        IEnumerable<IEntityTypeProjection> entityTypeProjections)
    {
        EnsureArg.IsNotNull(entityTypeProjections, nameof(entityTypeProjections));

        var entityType = typeof(TEntity);
        _projectFuncMap = entityTypeProjections
            .SelectMany(projection => CreateProjectFuncMap(projection, entityType))
            .ToFrozenDictionary(item => (item.EntityType, item.ResultType), item => item.ProjectFunc);

        _entityTypeHierarchy = BuildEntityTypeHierarchy(entityType);
    }

    /// <summary>
    /// Projects the results of the specified entity query to a new type using a registered mapping function.
    /// </summary>
    /// <typeparam name="TResult">The type to which the entities are projected.</typeparam>
    /// <param name="entityQuery">The <see cref="IQueryable{TEntity}"/> collection of entities to project.</param>
    /// <param name="dbContext">The <see cref="DbContext"/> used to resolve projection mappings.</param>
    /// <returns>An <see cref="IQueryable{TResult}"/> representing the projected  results of the query.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no projection mapping is registered from <typeparamref name="TEntity"/> (or any of its base types) to the specified <typeparamref name="TResult"/>.
    /// </exception>
    public IQueryable<TResult> ProjectTo<TResult>(IQueryable<TEntity> entityQuery, DbContext dbContext)
    {
        EnsureArg.IsNotNull(entityQuery, nameof(entityQuery));
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));

        var projectionContext = new ProjectionContext(dbContext);
        var resultType = typeof(TResult);

        foreach (var entityType in _entityTypeHierarchy)
        {
            if (_projectFuncMap.TryGetValue((entityType, resultType), out var projectFunc))
            {
                return (IQueryable<TResult>)projectFunc(entityQuery, projectionContext);
            }
        }

        throw new InvalidOperationException($"No projection specified from '{typeof(TEntity).FullName}' (or any of its base types) to '{resultType.FullName}'.");
    }

    private static List<Type> BuildEntityTypeHierarchy(Type entityType)
    {
        var hierarchy = new List<Type>();
        var currentType = entityType;

        while (currentType != typeof(object) && currentType is not null)
        {
            hierarchy.Add(currentType);
            currentType = currentType.BaseType;
        }

        return hierarchy;
    }

    private static List<(Type EntityType, Type ResultType, ProjectFunc ProjectFunc)> CreateProjectFuncMap(
        IEntityTypeProjection entityTypeProjection,
        Type targetEntityType)
    {
        var projectionType = entityTypeProjection.GetType();
        var projectionsFound = new List<(Type EntityType, Type ResultType, ProjectFunc ProjectFunc)>();
        var hasAnyProjectionInterface = false;

        foreach (var interfaceType in projectionType.GetInterfaces())
        {
            if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == ProjectionInterfaceType)
            {
                hasAnyProjectionInterface = true;
                var typeArguments = interfaceType.GetGenericArguments();
                var entityType = typeArguments[0];

                if (entityType.IsAssignableFrom(targetEntityType))
                {
                    var resultType = typeArguments[1];
                    projectionsFound.Add((entityType, resultType, GetOrCompileProjectFunc(entityTypeProjection, entityType, resultType)));
                }
            }
        }

        return hasAnyProjectionInterface
            ? projectionsFound
            : throw new ArgumentOutOfRangeException(
                nameof(entityTypeProjection),
                entityTypeProjection,
                $"Entity type projection of the {projectionType.FullName} type is incomplete. Consider implementing at least one projection.");
    }
}
