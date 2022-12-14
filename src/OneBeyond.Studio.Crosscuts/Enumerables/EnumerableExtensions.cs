using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OneBeyond.Studio.Crosscuts.Enumerables;

/// <summary>
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Produces <see cref="IEnumerable{T}"/> consisting of a single element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? @this)
        => @this is null || !@this.Any();
}
