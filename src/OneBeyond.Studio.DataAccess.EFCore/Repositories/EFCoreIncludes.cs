using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OneBeyond.Studio.Application.SharedKernel.Specifications;

namespace OneBeyond.Studio.DataAccess.EFCore.Repositories;

internal class EFCoreIncludes<TEntity> : IIncludes<TEntity>
    where TEntity : class
{
    private readonly IQueryable<TEntity> _query;

    public EFCoreIncludes(IQueryable<TEntity> query)
    {
        EnsureArg.IsNotNull(query, nameof(query));

        _query = query;
    }

    public IQueryable<TEntity> GetQuery()
        => _query;

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, TChild>> navigation)
        where TChild : class
    {
        EnsureArg.IsNotNull(navigation, nameof(navigation));

        var query = GetQuery().Include(navigation);

        return new Includes<TEntity, TChild>(query);
    }

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, IEnumerable<TChild>>> navigation)
        where TChild : class
    {
        EnsureArg.IsNotNull(navigation, nameof(navigation));

        var query = GetQuery().Include(navigation);

        return new Includes<TEntity, TChild>(query);
    }

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, ICollection<TChild>>> navigation)
        where TChild : class
    {
        EnsureArg.IsNotNull(navigation, nameof(navigation));

        var query = GetQuery().Include(navigation);

        return new Includes<TEntity, TChild>(query);
    }

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, IReadOnlyCollection<TChild>>> navigation)
        where TChild : class
    {
        EnsureArg.IsNotNull(navigation, nameof(navigation));

        var query = GetQuery().Include(navigation);

        return new Includes<TEntity, TChild>(query);
    }
}

internal sealed class Includes<TEntity, TChild>
    : EFCoreIncludes<TEntity>
    , IIncludes<TEntity, TChild>
    where TEntity : class
{
    private readonly IIncludableQueryable<TEntity, TChild>? _query1;
    private readonly IIncludableQueryable<TEntity, IEnumerable<TChild>>? _query2;

    public Includes(IIncludableQueryable<TEntity, TChild> query)
        : base(query)
    {
        _query1 = query;
        _query2 = default;
    }

    public Includes(IIncludableQueryable<TEntity, IEnumerable<TChild>> query)
        : base(query)
    {
        _query1 = default;
        _query2 = query;
    }

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, TNextChild>> navigation)
        where TNextChild : class
    {
        EnsureArg.IsNotNull(navigation, nameof(navigation));

        var query = _query1 is null
            ? _query2!.ThenInclude(navigation)
            : _query1.ThenInclude(navigation);

        return new Includes<TEntity, TNextChild>(query);
    }

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, IEnumerable<TNextChild>>> navigation)
        where TNextChild : class
    {
        EnsureArg.IsNotNull(navigation, nameof(navigation));

        var query = _query1 is null
            ? _query2!.ThenInclude(navigation)
            : _query1.ThenInclude(navigation);

        return new Includes<TEntity, TNextChild>(query);
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
        => throw new InvalidOperationException($"Consider using {nameof(EFPlusIncludes<TEntity>)} includes");
}
