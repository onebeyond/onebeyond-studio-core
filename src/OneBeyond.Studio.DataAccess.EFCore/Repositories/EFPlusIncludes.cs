using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.Expressions;
using OneBeyond.Studio.Crosscuts.Reflection;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using Z.EntityFramework.Plus;

#nullable disable

namespace OneBeyond.Studio.DataAccess.EFCore.Repositories;

internal class EFPlusIncludes<TEntity> : IIncludes<TEntity>
    where TEntity : class
{
    private static readonly MethodInfo EnumerableWhereGenericMethodInfo = Reflector
        .MethodFrom(() => Enumerable.Where<object>(default, _ => true))
        .GetGenericMethodDefinition();
    private static readonly ConcurrentDictionary<Type, MethodInfo> EnumerableWhereMethodInfoList = new();

    private readonly IQueryable<TEntity> _query;

    public EFPlusIncludes(IQueryable<TEntity> query)
    {
        EnsureArg.IsNotNull(query, nameof(query));

        _query = query;
    }

    public virtual IQueryable<TEntity> GetQuery()
        => _query;

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, TChild>> navigation)
        where TChild : class
        => new EFPlusIncludes<TEntity, TChild>(
            GetQuery(),
            navigation);

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, IEnumerable<TChild>>> navigation)
        where TChild : class
        => new EFPlusIncludes<TEntity, TChild>(
            GetQuery(),
            navigation);

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, ICollection<TChild>>> navigation)
        where TChild : class
        => Include(
            Expression.Lambda<Func<TEntity, IEnumerable<TChild>>>(
                EnsureArg.IsNotNull(navigation, nameof(navigation)).Body,
                navigation.Parameters));

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, IReadOnlyCollection<TChild>>> navigation)
        where TChild : class
        => Include(
            Expression.Lambda<Func<TEntity, IEnumerable<TChild>>>(
                EnsureArg.IsNotNull(navigation, nameof(navigation)).Body,
                navigation.Parameters));

    protected static MethodInfo GetEnumerableWhereMethodInfo(Type type)
        => EnumerableWhereMethodInfoList.GetOrAdd(
            type,
            (_) => EnumerableWhereGenericMethodInfo.MakeGenericMethod(type));
}

internal sealed class EFPlusIncludes<TEntity, TChild>
    : EFPlusIncludes<TEntity>
    , IIncludes<TEntity, TChild>
    where TEntity : class
    where TChild : class
{
    private readonly List<Expression<Func<TChild, bool>>> _predicates;
    private readonly Lazy<Expression<Func<TEntity, IEnumerable<TChild>>>> _includeFilter1;
    private readonly Expression<Func<TEntity, TChild>> _includeFilter2;

    public EFPlusIncludes(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, IEnumerable<TChild>>> navigation)
        : base(query)
    {
        _predicates = new List<Expression<Func<TChild, bool>>>();
        _includeFilter1 = new Lazy<Expression<Func<TEntity, IEnumerable<TChild>>>>(
            () =>
            {
                return Expression.Lambda<Func<TEntity, IEnumerable<TChild>>>(
                    _predicates
                        .Aggregate(
                            navigation.Body,
                            (body, predicate) =>
                            {
                                var itemType = predicate.Parameters[0].Type;
                                var enumerableWhereMethodInfo = GetEnumerableWhereMethodInfo(itemType);
                                var enumerableWhereCall = Expression.Call(enumerableWhereMethodInfo, body, predicate);
                                return enumerableWhereCall;
                            }),
                    navigation.Parameters);
            });
        _includeFilter2 = default;
    }

    public EFPlusIncludes(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TChild>> navigation)
        : base(query)
    {
        _predicates = default;
        _includeFilter1 = default;
        _includeFilter2 = navigation;
    }

    public override IQueryable<TEntity> GetQuery()
    {
        var query = base.GetQuery();
        return _includeFilter1 is null
            ? query.IncludeFilter(_includeFilter2)
            : query.IncludeFilter(_includeFilter1.Value);
    }

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, TNextChild>> navigation)
        where TNextChild : class
    {
        return _includeFilter1 is null
            ? new EFPlusIncludes<TEntity, TNextChild, TChild>(
                GetQuery(),
                _includeFilter2,
                navigation)
            : new EFPlusIncludes<TEntity, TNextChild, TChild>(
                GetQuery(),
                _includeFilter1.Value,
                navigation);
    }

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, IEnumerable<TNextChild>>> navigation)
        where TNextChild : class
    {
        return _includeFilter1 is null
            ? new EFPlusIncludes<TEntity, TNextChild, TChild>(
                GetQuery(),
                _includeFilter2,
                navigation)
            : new EFPlusIncludes<TEntity, TNextChild, TChild>(
                GetQuery(),
                _includeFilter1.Value,
                navigation);
    }

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, ICollection<TNextChild>>> navigation)
        where TNextChild : class
        => ThenInclude(
            Expression.Lambda<Func<TChild, IEnumerable<TNextChild>>>(
                EnsureArg.IsNotNull(navigation, nameof(navigation)).Body,
                navigation.Parameters));

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, IReadOnlyCollection<TNextChild>>> navigation)
        where TNextChild : class
        => ThenInclude(
            Expression.Lambda<Func<TChild, IEnumerable<TNextChild>>>(
                EnsureArg.IsNotNull(navigation, nameof(navigation)).Body,
                navigation.Parameters));

    public IIncludes<TEntity, TChild> Where(Expression<Func<TChild, bool>> predicate)
    {
        EnsureArg.IsNotNull(predicate, nameof(predicate));

        if (_predicates is null)
        {
            throw new InvalidOperationException(
                "Predicate cannot be applied to a navigation property referring to a single entity");
        }

        _predicates.Add(predicate);

        return this;
    }
}

internal sealed class EFPlusIncludes<TEntity, TChild, TParent>
    : EFPlusIncludes<TEntity>
    , IIncludes<TEntity, TChild>
    where TEntity : class
    where TChild : class
{
    private static readonly MethodInfo EnumerableSelectManyGenericMethodInfo = Reflector
        .MethodFrom(() => Enumerable.SelectMany<object, object>(default, _ => default))
        .GetGenericMethodDefinition();
    private static readonly MethodInfo EnumerableSelectGenericMethodInfo = Reflector
        .MethodFrom(() => Enumerable.Select<object, object>(default, _ => default))
        .GetGenericMethodDefinition();

    private readonly List<Expression<Func<TChild, bool>>> _predicates;
    private readonly Lazy<Expression<Func<TEntity, IEnumerable<TChild>>>> _includeFilter1;
    private readonly Expression<Func<TEntity, TChild>> _includeFilter2;

    public EFPlusIncludes(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, IEnumerable<TParent>>> includeFilter,
        Expression<Func<TParent, IEnumerable<TChild>>> navigation)
        : base(query)
    {
        _predicates = new List<Expression<Func<TChild, bool>>>();
        _includeFilter1 = new Lazy<Expression<Func<TEntity, IEnumerable<TChild>>>>(
            () =>
            {
                var selectClause = Expression.Lambda<Func<TParent, IEnumerable<TChild>>>(
                    _predicates
                        .Aggregate(
                            navigation.Body,
                            (body, predicate) =>
                            {
                                var itemType = predicate.Parameters[0].Type;
                                var enumerableWhereMethodInfo = GetEnumerableWhereMethodInfo(itemType);
                                var enumerableWhereCall = Expression.Call(
                                    enumerableWhereMethodInfo,
                                    body,
                                    predicate);
                                return enumerableWhereCall;
                            }),
                    navigation.Parameters);
                var enumerableSelectManyMethodInfo = EnumerableSelectManyGenericMethodInfo.MakeGenericMethod(
                    typeof(TParent),
                    typeof(TChild));
                var enumerableSelectManyCall = Expression.Call(
                    enumerableSelectManyMethodInfo,
                    includeFilter.Body,
                    selectClause);
                return Expression.Lambda<Func<TEntity, IEnumerable<TChild>>>(
                    enumerableSelectManyCall,
                    includeFilter.Parameters);
            });
        _includeFilter2 = default;
    }

    public EFPlusIncludes(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, IEnumerable<TParent>>> includeFilter,
        Expression<Func<TParent, TChild>> navigation)
        : base(query)
    {
        _predicates = default;
        _includeFilter1 = new Lazy<Expression<Func<TEntity, IEnumerable<TChild>>>>(
            () =>
            {
                var selectClause = Expression.Lambda<Func<TParent, TChild>>(
                    navigation.Body,
                    navigation.Parameters);
                var enumerableSelectMethodInfo = EnumerableSelectGenericMethodInfo.MakeGenericMethod(
                    typeof(TParent),
                    typeof(TChild));
                var enumerableSelectCall = Expression.Call(
                    enumerableSelectMethodInfo,
                    includeFilter.Body,
                    selectClause);
                return Expression.Lambda<Func<TEntity, IEnumerable<TChild>>>(
                    enumerableSelectCall,
                    includeFilter.Parameters);
            });
        _includeFilter2 = default;
    }

    public EFPlusIncludes(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TParent>> includeFilter,
        Expression<Func<TParent, IEnumerable<TChild>>> navigation)
        : base(query)
    {
        _predicates = new List<Expression<Func<TChild, bool>>>();
        _includeFilter1 = new Lazy<Expression<Func<TEntity, IEnumerable<TChild>>>>(
            () =>
            {
                var extendedNavigation = Expression.Lambda<Func<TEntity, IEnumerable<TChild>>>(
                    navigation.Body.ReplaceExpression(
                        navigation.Parameters[0],
                        includeFilter.Body),
                    includeFilter.Parameters[0]);
                return Expression.Lambda<Func<TEntity, IEnumerable<TChild>>>(
                    _predicates
                        .Aggregate(
                            extendedNavigation.Body,
                            (body, predicate) =>
                            {
                                var itemType = predicate.Parameters[0].Type;
                                var enumerableWhereMethodInfo = GetEnumerableWhereMethodInfo(itemType);
                                var enumerableWhereCall = Expression.Call(enumerableWhereMethodInfo, body, predicate);
                                return enumerableWhereCall;
                            }),
                    extendedNavigation.Parameters);
            });
        _includeFilter2 = default;
    }

    public EFPlusIncludes(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TParent>> includeFilter,
        Expression<Func<TParent, TChild>> navigation)
        : base(query)
    {
        _predicates = default;
        _includeFilter1 = default;
        _includeFilter2 = Expression.Lambda<Func<TEntity, TChild>>(
            navigation.Body.ReplaceExpression(
                navigation.Parameters[0],
                includeFilter.Body),
            includeFilter.Parameters[0]);
    }

    public override IQueryable<TEntity> GetQuery()
    {
        var query = base.GetQuery();
        return _includeFilter1 is null
            ? query.IncludeFilter(_includeFilter2)
            : query.IncludeFilter(_includeFilter1.Value);
    }

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, TNextChild>> navigation)
        where TNextChild : class
    {
        return _includeFilter1 is null
            ? new EFPlusIncludes<TEntity, TNextChild, TChild>(
                GetQuery(),
                _includeFilter2,
                navigation)
            : new EFPlusIncludes<TEntity, TNextChild, TChild>(
                GetQuery(),
                _includeFilter1.Value,
                navigation);
    }

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, IEnumerable<TNextChild>>> navigation)
        where TNextChild : class
    {
        return _includeFilter1 is null
            ? new EFPlusIncludes<TEntity, TNextChild, TChild>(
                GetQuery(),
                _includeFilter2,
                navigation)
            : new EFPlusIncludes<TEntity, TNextChild, TChild>(
                GetQuery(),
                _includeFilter1.Value,
                navigation);
    }

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, ICollection<TNextChild>>> navigation)
        where TNextChild : class
        => ThenInclude(
            Expression.Lambda<Func<TChild, IEnumerable<TNextChild>>>(
                EnsureArg.IsNotNull(navigation, nameof(navigation)).Body,
                navigation.Parameters));

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, IReadOnlyCollection<TNextChild>>> navigation)
        where TNextChild : class
        => ThenInclude(
            Expression.Lambda<Func<TChild, IEnumerable<TNextChild>>>(
                EnsureArg.IsNotNull(navigation, nameof(navigation)).Body,
                navigation.Parameters));

    public IIncludes<TEntity, TChild> Where(Expression<Func<TChild, bool>> predicate)
    {
        EnsureArg.IsNotNull(predicate, nameof(predicate));

        if (_predicates is null)
        {
            throw new InvalidOperationException(
                "Predicate cannot be applied to a navigation property referring to a single entity");
        }

        _predicates.Add(predicate);

        return this;
    }
}
