using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.Specifications;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Specifications;

internal class TestIncludes<TClass> : IIncludes<TClass>
{
    public TestIncludes(IList<(Expression, IList<Expression>)> includeList)
    {
        EnsureArg.IsNotNull(includeList, nameof(includeList));

        IncludeList = includeList;
    }

    protected IList<(Expression, IList<Expression>)> IncludeList { get; }

    public IIncludes<TClass, TChild> Include<TChild>(
        Expression<Func<TClass, TChild>> navigation)
        where TChild : class
    {
        IncludeList.Add((navigation, new List<Expression>()));
        return new TestIncludes<TClass, TChild>(IncludeList);
    }

    public IIncludes<TClass, TChild> Include<TChild>(
        Expression<Func<TClass, IEnumerable<TChild>>> navigation)
        where TChild : class
    {
        IncludeList.Add((navigation, new List<Expression>()));
        return new TestIncludes<TClass, TChild>(IncludeList);
    }

    public IIncludes<TClass, TChild> Include<TChild>(
        Expression<Func<TClass, ICollection<TChild>>> navigation)
        where TChild : class
    {
        IncludeList.Add((navigation, new List<Expression>()));
        return new TestIncludes<TClass, TChild>(IncludeList);
    }

    public IIncludes<TClass, TChild> Include<TChild>(
        Expression<Func<TClass, IReadOnlyCollection<TChild>>> navigation)
        where TChild : class
    {
        IncludeList.Add((navigation, new List<Expression>()));
        return new TestIncludes<TClass, TChild>(IncludeList);
    }
}

internal class TestIncludes<TClass, TChild> : TestIncludes<TClass>, IIncludes<TClass, TChild>
{
    public TestIncludes(IList<(Expression, IList<Expression>)> includeList)
        : base(includeList)
    {
    }

    public IIncludes<TClass, TNexTChild> ThenInclude<TNexTChild>(
        Expression<Func<TChild, TNexTChild>> navigation)
        where TNexTChild : class
    {
        IncludeList.Add((navigation, new List<Expression>()));
        return new TestIncludes<TClass, TNexTChild>(IncludeList);
    }

    public IIncludes<TClass, TNexTChild> ThenInclude<TNexTChild>(
        Expression<Func<TChild, IEnumerable<TNexTChild>>> navigation)
        where TNexTChild : class
    {
        IncludeList.Add((navigation, new List<Expression>()));
        return new TestIncludes<TClass, TNexTChild>(IncludeList);
    }

    public IIncludes<TClass, TNexTChild> ThenInclude<TNexTChild>(
        Expression<Func<TChild, ICollection<TNexTChild>>> navigation)
        where TNexTChild : class
    {
        IncludeList.Add((navigation, new List<Expression>()));
        return new TestIncludes<TClass, TNexTChild>(IncludeList);
    }

    public IIncludes<TClass, TNexTChild> ThenInclude<TNexTChild>(
        Expression<Func<TChild, IReadOnlyCollection<TNexTChild>>> navigation)
        where TNexTChild : class
    {
        IncludeList.Add((navigation, new List<Expression>()));
        return new TestIncludes<TClass, TNexTChild>(IncludeList);
    }

    public IIncludes<TClass, TChild> Where(Expression<Func<TChild, bool>> predicate)
    {
        IncludeList[IncludeList.Count - 1].Item2.Add(predicate);
        return this;
    }
}
