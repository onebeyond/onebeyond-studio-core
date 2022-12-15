using System;
using System.Linq.Expressions;

namespace OneBeyond.Studio.Domain.SharedKernel.DataAccessPolicies;

/// <summary>
/// </summary>
public static class DataAccessPolicy
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="criteria"></param>
    /// <param name="function"></param>
    /// <returns></returns>
    public static DataAccessPolicy<TEntity> Create<TEntity>(
        Expression<Func<TEntity, bool>>? criteria,
        Func<TEntity, bool>? function)
        => new DataAccessPolicy<TEntity>(criteria, function);
}

/// <summary>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class DataAccessPolicy<TEntity>
{
    /// <summary>
    /// </summary>
    /// <param name="criteria"></param>
    /// <param name="function"></param>
    public DataAccessPolicy(Expression<Func<TEntity, bool>>? criteria, Func<TEntity, bool>? function)
    {
        CanBeAccessedCriteria = criteria;
        CanBeAccessedFunction = function;
    }

    /// <summary>
    /// </summary>
    public Expression<Func<TEntity, bool>>? CanBeAccessedCriteria { get; }

    /// <summary>
    /// </summary>
    public Func<TEntity, bool>? CanBeAccessedFunction { get; }
}
