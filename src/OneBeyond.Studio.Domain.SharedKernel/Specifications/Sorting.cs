using System;
using System.ComponentModel;
using System.Linq.Expressions;
using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.Specifications;

/// <summary>
/// </summary>
public static class Sorting
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sortBy"></param>
    /// <returns></returns>
    public static Sorting<T> CreateAscending<T>(Expression<Func<T, object?>> sortBy)
        => new Sorting<T>(sortBy, ListSortDirection.Ascending);

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sortBy"></param>
    /// <returns></returns>
    public static Sorting<T> CreateDescending<T>(Expression<Func<T, object?>> sortBy)
        => new Sorting<T>(sortBy, ListSortDirection.Descending);
}

/// <summary>
/// </summary>
/// <typeparam name="T"></typeparam>
public class Sorting<T>
{
    /// <summary>
    /// </summary>
    /// <param name="sortBy"></param>
    /// <param name="direction"></param>
    internal Sorting(Expression<Func<T, object?>> sortBy, ListSortDirection direction)
    {
        EnsureArg.IsNotNull(sortBy, nameof(sortBy));

        SortBy = sortBy;
        Direction = direction;
    }

    /// <summary>
    /// </summary>
    public Expression<Func<T, object?>> SortBy { get; }

    /// <summary>
    /// </summary>
    public ListSortDirection Direction { get; }
}
