using System;
using System.Collections.Generic;
using EnsureThat;

namespace OneBeyond.Studio.Crosscuts.Comparers;

/// <summary>
/// </summary>
public static class Comparer
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IComparer<T> Create<T>(Func<T?, T?, int> comparer)
        => new ComparerByDelegate<T>(comparer);

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IComparer<T> ToComparer<T>(this Func<T?, T?, int> comparer)
        => new ComparerByDelegate<T>(comparer);

    private sealed class ComparerByDelegate<T> : Comparer<T>
    {
        private readonly Func<T?, T?, int> _comparer;

        public ComparerByDelegate(Func<T?, T?, int> comparer)
        {
            EnsureArg.IsNotNull(comparer, nameof(comparer));
            _comparer = comparer;
        }

        public override int Compare(T? x, T? y)
            => _comparer(x, y);
    }
}
