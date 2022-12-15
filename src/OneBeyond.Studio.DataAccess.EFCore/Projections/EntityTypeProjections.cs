using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using OneBeyond.Studio.Crosscuts.Reflection;

namespace OneBeyond.Studio.DataAccess.EFCore.Projections;

internal class EntityTypeProjections<TEntity> : IEntityTypeProjections<TEntity>
    where TEntity : class
{
    private static readonly MethodInfo DoProjectMethodInfo = Reflector
        .MethodFrom(() => DoProject<object>(default!, default!, default!))
        .GetGenericMethodDefinition();

    private readonly IReadOnlyDictionary<Type, DoProjectFunc> _doProjectFuncMap;
    private readonly IConfigurationProvider _mapperConfigurationProvider;

    public EntityTypeProjections(
        IEnumerable<IEntityTypeProjection<TEntity>> entityTypeProjections,
        IConfigurationProvider mapperConfigurationProvider)
    {
        EnsureArg.IsNotNull(entityTypeProjections, nameof(entityTypeProjections));
        EnsureArg.IsNotNull(mapperConfigurationProvider, nameof(mapperConfigurationProvider));

        _doProjectFuncMap = entityTypeProjections
            .SelectMany(CreateDoProjectFuncMap)
            .ToDictionary((item) => item.ResultType, (item) => item.DoProject);
        _mapperConfigurationProvider = mapperConfigurationProvider;
    }

    public IQueryable<TResult> ProjectTo<TResult>(IQueryable<TEntity> entityQuery, DbContext dbContext)
    {
        EnsureArg.IsNotNull(entityQuery, nameof(entityQuery));
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));

        var projectionContext = new ProjectionContext(dbContext, _mapperConfigurationProvider);
        return _doProjectFuncMap.TryGetValue(typeof(TResult), out var doProject)
            ? (IQueryable<TResult>)doProject(
                entityQuery,
                projectionContext)
            : DoProjectDefault<TResult>(entityQuery, projectionContext);
    }

    public virtual IQueryable<TResult> DoProjectDefault<TResult>(
        IQueryable<TEntity> entityQuery,
        ProjectionContext context)
        => entityQuery.ProjectTo<TResult>(_mapperConfigurationProvider);

    private static IReadOnlyCollection<(Type ResultType, DoProjectFunc DoProject)> CreateDoProjectFuncMap(
        IEntityTypeProjection<TEntity> entityTypeProjection)
    {
        var doProjectFuncMap = entityTypeProjection.GetType().GetInterfaces()
            .Where((interfaceType) =>
                interfaceType.IsGenericType
                && interfaceType.GetGenericTypeDefinition() == typeof(IEntityTypeProjection<,>))
            .Select((interfaceType) => interfaceType.GetGenericArguments()[1])
            .Select((resultType) => (resultType, CompileDoProjectFunc(entityTypeProjection, resultType)))
            .ToList();
        return doProjectFuncMap.Count == 0
            ? throw new ArgumentOutOfRangeException(
                nameof(entityTypeProjection),
                $"Entity type projection of the {entityTypeProjection.GetType().FullName} type is incomplete. " +
                "Consider implementing at least one projection.")
            : doProjectFuncMap;
    }

    private static DoProjectFunc CompileDoProjectFunc(
        IEntityTypeProjection<TEntity> entityTypeProjection,
        Type resultType)
    {
        var interfaceType = typeof(IEntityTypeProjection<,>).MakeGenericType(typeof(TEntity), resultType);
        var projectionInstance = Expression.Constant(entityTypeProjection, interfaceType);
        var entityQueryParam = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(typeof(TEntity)), "entityQuery");
        var projectionContextParam = Expression.Parameter(typeof(ProjectionContext), "context");
        var doProjectCall = Expression.Call(
            DoProjectMethodInfo.MakeGenericMethod(resultType),
            projectionInstance,
            entityQueryParam,
            projectionContextParam);
        var doProjectLambda = Expression.Lambda<DoProjectFunc>(
            doProjectCall,
            entityQueryParam,
            projectionContextParam);
        return doProjectLambda.Compile();
    }

    private static IQueryable<TResult> DoProject<TResult>(
        IEntityTypeProjection<TEntity, TResult> entityTypeProjection,
        IQueryable<TEntity> enityQuery,
        ProjectionContext context)
        => entityTypeProjection.Project(enityQuery, context);

    private delegate object DoProjectFunc(IQueryable<TEntity> entityQuery, ProjectionContext context);
}
