using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OneBeyond.Studio.Domain.SharedKernel.Specifications;

namespace OneBeyond.Studio.DataAccess.EFCore.Repositories;

internal class IncludesTraits<TEntity>
    : IIncludes<TEntity>
    where TEntity : class
{
    public IncludesTraits()
        : this(false)
    {
    }

    protected IncludesTraits(bool haveWhereClause)
    {
        HaveWhereClause = haveWhereClause;
    }

    public bool HaveWhereClause { get; protected set; }

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, TChild>> navigation)
        where TChild : class
        => new IncludesTraits<TEntity, TChild>(HaveWhereClause);

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, IEnumerable<TChild>>> navigation)
        where TChild : class
        => new IncludesTraits<TEntity, TChild>(HaveWhereClause);

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, ICollection<TChild>>> navigation)
        where TChild : class
        => new IncludesTraits<TEntity, TChild>(HaveWhereClause);

    public IIncludes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, IReadOnlyCollection<TChild>>> navigation)
        where TChild : class
        => new IncludesTraits<TEntity, TChild>(HaveWhereClause);
}

internal sealed class IncludesTraits<TEntity, TChild>
    : IncludesTraits<TEntity>
    , IIncludes<TEntity, TChild>
    where TEntity : class
{
    public IncludesTraits(bool haveWhereClause)
        : base(haveWhereClause)
    {
    }

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, TNextChild>> navigation)
        where TNextChild : class
        => new IncludesTraits<TEntity, TNextChild>(HaveWhereClause);

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, IEnumerable<TNextChild>>> navigation)
        where TNextChild : class
        => new IncludesTraits<TEntity, TNextChild>(HaveWhereClause);

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, ICollection<TNextChild>>> navigation)
        where TNextChild : class
        => new IncludesTraits<TEntity, TNextChild>(HaveWhereClause);

    public IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, IReadOnlyCollection<TNextChild>>> navigation)
        where TNextChild : class
        => new IncludesTraits<TEntity, TNextChild>(HaveWhereClause);

    public IIncludes<TEntity, TChild> Where(Expression<Func<TChild, bool>> predicate)
    {
        HaveWhereClause = true;
        return this;
    }
}
