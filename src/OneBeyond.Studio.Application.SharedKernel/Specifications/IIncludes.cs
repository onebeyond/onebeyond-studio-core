using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OneBeyond.Studio.Application.SharedKernel.Specifications;

/// <summary>
/// Defines related object graph by navigation properties.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IIncludes<TEntity>
{
    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// The <see cref="IIncludes{TObject, TProperty}.ThenInclude{TNextProperty}(Expression{Func{TProperty, TNextProperty}})"/> method
    /// can be used for further building the graph.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    IIncludes<TEntity, TChild> Include<TChild>(Expression<Func<TEntity, TChild>> navigation)
        where TChild : class;

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// The <see cref="IIncludes{TObject, TProperty}.ThenInclude{TNextProperty}(Expression{Func{TProperty, TNextProperty}})"/> method
    /// can be used for further building the graph.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    IIncludes<TEntity, TChild> Include<TChild>(Expression<Func<TEntity, IEnumerable<TChild>>> navigation)
        where TChild : class;

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// The <see cref="IIncludes{TObject, TProperty}.ThenInclude{TNextProperty}(Expression{Func{TProperty, TNextProperty}})"/> method
    /// can be used for further building the graph.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    IIncludes<TEntity, TChild> Include<TChild>(Expression<Func<TEntity, ICollection<TChild>>> navigation)
        where TChild : class;

    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// The <see cref="IIncludes{TObject, TProperty}.ThenInclude{TNextProperty}(Expression{Func{TProperty, TNextProperty}})"/> method
    /// can be used for further building the graph.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    IIncludes<TEntity, TChild> Include<TChild>(Expression<Func<TEntity, IReadOnlyCollection<TChild>>> navigation)
        where TChild : class;
}

/// <summary>
/// Defines related object graph by navigation properties.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TChild"></typeparam>
public interface IIncludes<TEntity, TChild> : IIncludes<TEntity>
{
    /// <summary>
    /// Adds a related object into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(Expression<Func<TChild, TNextChild>> navigation)
        where TNextChild : class;

    /// <summary>
    /// Adds a related property into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(Expression<Func<TChild, IEnumerable<TNextChild>>> navigation)
        where TNextChild : class;

    /// <summary>
    /// Adds a related property into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(Expression<Func<TChild, ICollection<TNextChild>>> navigation)
        where TNextChild : class;

    /// <summary>
    /// Adds a related property into the graph via a navigation property.
    /// </summary>
    /// <typeparam name="TNextChild"></typeparam>
    /// <param name="navigation"></param>
    /// <returns></returns>
    IIncludes<TEntity, TNextChild> ThenInclude<TNextChild>(Expression<Func<TChild, IReadOnlyCollection<TNextChild>>> navigation)
        where TNextChild : class;

    /// <summary>
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IIncludes<TEntity, TChild> Where(Expression<Func<TChild, bool>> predicate);
}
