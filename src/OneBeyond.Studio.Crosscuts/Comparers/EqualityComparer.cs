using System;
using System.Collections.Generic;
using EnsureThat;

namespace OneBeyond.Studio.Crosscuts.Comparers;

/// <summary>
/// </summary>
public static class EqualityComparer
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="compare"></param>
    /// <param name="getHash"></param>
    /// <returns></returns>
    public static IEqualityComparer<T> Create<T>(
        Func<T?, T?, bool> compare,
        Func<T, int>? getHash = default)
        where T : notnull
        => new EqualityComparerByDelegate<T>(compare, getHash ?? GetHashDefault);

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="compare"></param>
    /// <param name="getHash"></param>
    /// <returns></returns>
    public static IEqualityComparer<T> ToEqualityComparer<T>(
        this Func<T?, T?, bool> compare,
        Func<T, int>? getHash = default)
        where T : notnull
        => new EqualityComparerByDelegate<T>(compare, getHash ?? GetHashDefault);

    private sealed class EqualityComparerByDelegate<T> : EqualityComparer<T>
    {
        private readonly Func<T?, T?, bool> _compare;
        private readonly Func<T, int> _getHash;

        public EqualityComparerByDelegate(Func<T?, T?, bool> compare, Func<T, int> getHash)
        {
            EnsureArg.IsNotNull(compare, nameof(compare));
            EnsureArg.IsNotNull(getHash, nameof(getHash));

            _compare = compare;
            _getHash = getHash;
        }

        public override bool Equals(T? x, T? y)
            => _compare(x, y);

        public override int GetHashCode(T obj)
            => _getHash(obj);
    }

    private static int GetHashDefault<T>(T value)
        where T : notnull
        => value.GetHashCode();
}
