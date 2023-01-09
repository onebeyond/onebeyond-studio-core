using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EnsureThat;

namespace OneBeyond.Studio.Application.SharedKernel.Specifications;

/// <summary>
/// </summary>
public static class Includes
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="includes"></param>
    /// <returns></returns>
    public static Includes<TEntity> Create<TEntity>(params Expression<Func<TEntity, object>>[] includes)
    {
        EnsureArg.IsNotNull(includes, nameof(includes));

        return includes.Aggregate(
            new Includes<TEntity>(false),
            (includes, include) => includes.Include(include));
    }
}

/// <summary>
/// Defines related object graph by navigation properties.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class Includes<TEntity> : IIncludes<TEntity>
{
    /// <param name="haveCartesianExplosion">
    /// Gives a hint on the fact that produced includes chain is a subject to
    /// the <see href="https://www.thinktecture.com/en/entity-framework-core/cartesian-explosion-problem-in-3-1/">Cartesian Explosion</see> problem.
    /// This hint allows its mitigation by enabling the <see href="https://docs.microsoft.com/en-us/ef/core/querying/single-split-queries">Query Splitting</see>
    /// feature of EF Core.
    /// </param>
    public Includes(bool haveCartesianExplosion = false)
    {
        HaveCartesianExplosion = haveCartesianExplosion;
    }

    public bool HaveCartesianExplosion { get; }

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// The <see cref="Includes{TObject, TProperty}.ThenInclude{TNextChild}(Expression{Func{TProperty, TNextChild}})"/> method
    /// can be used for further building the graph.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public Includes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, TChild>> navigation)
        where TChild : class
        => new(this, navigation);

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// The <see cref="Includes{TObject, TProperty}.ThenInclude{TNextChild}(Expression{Func{TProperty, TNextChild}})"/> method
    /// can be used for further building the graph.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public Includes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, IEnumerable<TChild>>> navigation)
        where TChild : class
        => new(this, navigation);

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// The <see cref="Includes{TObject, TProperty}.ThenInclude{TNextChild}(Expression{Func{TProperty, TNextChild}})"/> method
    /// can be used for further building the graph.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public Includes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, ICollection<TChild>>> navigation)
        where TChild : class
        => new(
            this,
            Expression.Lambda<Func<TEntity, IEnumerable<TChild>>>(
                navigation.Body,
                navigation.Parameters));

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// The <see cref="Includes{TObject, TProperty}.ThenInclude{TNextChild}(Expression{Func{TProperty, TNextChild}})"/> method
    /// can be used for further building the graph.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public Includes<TEntity, TChild> Include<TChild>(
        Expression<Func<TEntity, IReadOnlyCollection<TChild>>> navigation)
        where TChild : class
        => new(
            this,
            Expression.Lambda<Func<TEntity, IEnumerable<TChild>>>(
                navigation.Body,
                navigation.Parameters));

    /// <summary>
    /// Replays the selected graph on another one.
    /// </summary>
    /// <typeparam name="TIncludes"></typeparam>
    /// <param name="includes"></param>
    /// <returns></returns>
    public virtual TIncludes Replay<TIncludes>(TIncludes includes)
        where TIncludes : class, IIncludes<TEntity>
        => includes;

    IIncludes<TEntity, TChild> IIncludes<TEntity>.Include<TChild>(
        Expression<Func<TEntity, TChild>> navigation)
        => Include(navigation);

    IIncludes<TEntity, TChild> IIncludes<TEntity>.Include<TChild>(
        Expression<Func<TEntity, IEnumerable<TChild>>> navigation)
        => Include(navigation);

    IIncludes<TEntity, TChild> IIncludes<TEntity>.Include<TChild>(
        Expression<Func<TEntity, ICollection<TChild>>> navigation)
        => Include(navigation);

    IIncludes<TEntity, TChild> IIncludes<TEntity>.Include<TChild>(
        Expression<Func<TEntity, IReadOnlyCollection<TChild>>> navigation)
        => Include(navigation);
}

/// <summary>
/// Defines related object graph by navigation properties.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TChild"></typeparam>
public class Includes<TEntity, TChild> : Includes<TEntity>, IIncludes<TEntity, TChild>
    where TChild : class
{
    private readonly Expression<Func<TEntity, TChild>>? _navigation1;
    private readonly Expression<Func<TEntity, IEnumerable<TChild>>>? _navigation2;
    private readonly List<Expression<Func<TChild, bool>>> _predicates;

    internal Includes(Includes<TEntity> previous, Expression<Func<TEntity, TChild>> navigation)
        : this(previous)
    {
        EnsureArg.IsNotNull(navigation, nameof(navigation));

        _navigation1 = navigation;
        _navigation2 = default;
    }

    internal Includes(Includes<TEntity> previous, Expression<Func<TEntity, IEnumerable<TChild>>> navigation)
        : this(previous)
    {
        EnsureArg.IsNotNull(navigation, nameof(navigation));

        _navigation1 = default;
        _navigation2 = navigation;
    }

    /// <summary>
    /// </summary>
    internal protected Includes(Includes<TEntity> previous)
        : base(previous.HaveCartesianExplosion)
    {
        EnsureArg.IsNotNull(previous, nameof(previous));

        _predicates = new List<Expression<Func<TChild, bool>>>();
        Previous = previous;
    }

    protected Includes<TEntity> Previous { get; }
    protected IEnumerable<Expression<Func<TChild, bool>>> Predicates => _predicates;

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public Includes<TEntity, TChild, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, TNextChild>> navigation)
        where TNextChild : class
        => new(this, navigation);

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public Includes<TEntity, TChild, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, IEnumerable<TNextChild>>> navigation)
        where TNextChild : class
        => new(this, navigation);

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public Includes<TEntity, TChild, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, ICollection<TNextChild>>> navigation)
        where TNextChild : class
        => new(
            this,
            Expression.Lambda<Func<TChild, IEnumerable<TNextChild>>>(
                navigation.Body,
                navigation.Parameters));

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public Includes<TEntity, TChild, TNextChild> ThenInclude<TNextChild>(
        Expression<Func<TChild, IReadOnlyCollection<TNextChild>>> navigation)
        where TNextChild : class
        => new(
            this,
            Expression.Lambda<Func<TChild, IEnumerable<TNextChild>>>(
                navigation.Body,
                navigation.Parameters));

    /// <summary>
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public Includes<TEntity, TChild> Where(Expression<Func<TChild, bool>> predicate)
    {
        _predicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Replays the selected graph on another one.
    /// </summary>
    /// <typeparam name="TIncludes"></typeparam>
    /// <param name="includes"></param>
    /// <returns></returns>
    public override TIncludes Replay<TIncludes>(TIncludes includes)
    {
        EnsureArg.IsNotNull(includes, nameof(includes));

        includes = Previous.Replay(includes);
        var updatedIncludes = _navigation1 == default
            ? includes.Include(_navigation2!)
            : includes.Include(_navigation1);
        updatedIncludes = Predicates.Aggregate(
            updatedIncludes,
            (result, predicate) => result.Where(predicate));
        return (TIncludes)updatedIncludes;
    }

    IIncludes<TEntity, TNextChild> IIncludes<TEntity, TChild>.ThenInclude<TNextChild>(
        Expression<Func<TChild, TNextChild>> navigation)
        => ThenInclude(navigation);

    IIncludes<TEntity, TNextChild> IIncludes<TEntity, TChild>.ThenInclude<TNextChild>(
        Expression<Func<TChild, IEnumerable<TNextChild>>> navigation)
        => ThenInclude(navigation);

    IIncludes<TEntity, TNextChild> IIncludes<TEntity, TChild>.ThenInclude<TNextChild>(
        Expression<Func<TChild, ICollection<TNextChild>>> navigation)
        => ThenInclude(navigation);

    IIncludes<TEntity, TNextChild> IIncludes<TEntity, TChild>.ThenInclude<TNextChild>(
        Expression<Func<TChild, IReadOnlyCollection<TNextChild>>> navigation)
        => ThenInclude(navigation);

    IIncludes<TEntity, TChild> IIncludes<TEntity, TChild>.Where(Expression<Func<TChild, bool>> predicate)
        => Where(predicate);
}

/// <summary>
/// Defines related object graph by navigation properties.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TChild"></typeparam>
/// <typeparam name="TNextChild"></typeparam>
public class Includes<TEntity, TChild, TNextChild> : Includes<TEntity, TNextChild>
    where TNextChild : class
{
    private readonly Expression<Func<TChild, TNextChild>>? _navigation1;
    private readonly Expression<Func<TChild, IEnumerable<TNextChild>>>? _navigation2;

    internal Includes(Includes<TEntity> previous, Expression<Func<TChild, TNextChild>> navigation)
        : base(previous)
    {
        EnsureArg.IsNotNull(navigation, nameof(navigation));

        _navigation1 = navigation;
        _navigation2 = default;
    }

    internal Includes(Includes<TEntity> previous, Expression<Func<TChild, IEnumerable<TNextChild>>> navigation)
        : base(previous)
    {
        EnsureArg.IsNotNull(navigation, nameof(navigation));

        _navigation1 = default;
        _navigation2 = navigation;
    }

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild1"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public new Includes<TEntity, TNextChild1> ThenInclude<TNextChild1>(
        Expression<Func<TNextChild, TNextChild1>> navigation)
        where TNextChild1 : class
        => new Includes<TEntity, TNextChild, TNextChild1>(this, navigation);

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild1"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public new Includes<TEntity, TNextChild1> ThenInclude<TNextChild1>(
        Expression<Func<TNextChild, IEnumerable<TNextChild1>>> navigation)
        where TNextChild1 : class
        => new Includes<TEntity, TNextChild, TNextChild1>(this, navigation);

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild1"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public new Includes<TEntity, TNextChild1> ThenInclude<TNextChild1>(
        Expression<Func<TNextChild, ICollection<TNextChild1>>> navigation)
        where TNextChild1 : class
        => new Includes<TEntity, TNextChild, TNextChild1>(
            this,
            Expression.Lambda<Func<TNextChild, IEnumerable<TNextChild1>>>(
                navigation.Body,
                navigation.Parameters));

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild1"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    public new Includes<TEntity, TNextChild1> ThenInclude<TNextChild1>(
        Expression<Func<TNextChild, IReadOnlyCollection<TNextChild1>>> navigation)
        where TNextChild1 : class
        => new Includes<TEntity, TNextChild, TNextChild1>(
            this,
            Expression.Lambda<Func<TNextChild, IEnumerable<TNextChild1>>>(
                navigation.Body,
                navigation.Parameters));

    /// <summary>
    /// Replays the selected graph on another one.
    /// </summary>
    /// <typeparam name="TIncludes"></typeparam>
    /// <param name="includes"></param>
    /// <returns></returns>
    public override TIncludes Replay<TIncludes>(TIncludes includes)
    {
        EnsureArg.IsNotNull(includes, nameof(includes));

        includes = Previous.Replay(includes);
        var updatedIncludes = _navigation1 == default
            ? ((IIncludes<TEntity, TChild>)includes).ThenInclude(_navigation2!)
            : ((IIncludes<TEntity, TChild>)includes).ThenInclude(_navigation1);
        updatedIncludes = Predicates.Aggregate(
            updatedIncludes,
            (result, predicate) => result.Where(predicate));
        return (TIncludes)updatedIncludes;
    }
}
