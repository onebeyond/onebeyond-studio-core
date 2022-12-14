using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Crosscuts.Logging;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities;

/// <summary>
/// source: https://github.com/jhewlett/ValueObject
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    private static readonly ILogger Logger = LogManager.CreateLogger<ValueObject>();

    private readonly Lazy<IReadOnlyList<PropertyInfo>> _properties;
    private readonly Lazy<IReadOnlyList<FieldInfo>> _fields;

    /// <summary>
    /// </summary>
    protected ValueObject()
    {
        _properties = new Lazy<IReadOnlyList<PropertyInfo>>(
            () =>
            {
                return GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null)
                    .ToList();
            });
        _fields = new Lazy<IReadOnlyList<FieldInfo>>(
            () =>
            {
                return GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null)
                    .ToList();
            });
    }

    /// <summary>
    /// Checks whether 2 value objects are the same based on their types and values
    /// of public properties and fields which are not marked by <see cref="IgnoreMemberAttribute"/>
    /// as well as on their types.
    /// Note: If a value object of one type compared with a value object of derived type,
    /// it gives a false result even though properties have the same values in part of base class.
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public static bool operator ==(ValueObject? obj1, ValueObject? obj2)
        => Equals(obj1, null)
            ? Equals(obj2, null)
            : obj1.Equals(obj2);

    /// <summary>
    /// Checks whether 2 value objects are different based on their types and values
    /// of public properties and fields which are not marked by <see cref="IgnoreMemberAttribute"/>
    /// as well as on their types.
    /// Note: If a value object of one type compared with a value object of derived type,
    /// it gives a false result even though properties have the same values in part of base class.
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public static bool operator !=(ValueObject? obj1, ValueObject? obj2)
        => !(obj1 == obj2);

    /// <summary>
    /// Checks whether 2 value objects are the same based on their types and values
    /// of public properties and fields which are not marked by <see cref="IgnoreMemberAttribute"/>
    /// as well as on their types.
    /// Note: If a value object of one type compared with a value object of derived type,
    /// it gives a false result even though properties have the same values in part of base class.
    /// <param name="obj"></param>
    /// <returns></returns>
    /// </summary>
    public bool Equals(ValueObject? obj)
        => Equals(obj as object);

    /// <summary>
    /// Checks whether 2 value objects are different based on their types and values
    /// of public properties and fields which are not marked by <see cref="IgnoreMemberAttribute"/>
    /// as well as on their types.
    /// Note: If a value object of one type compared with a value object of derived type,
    /// it gives a false result even though properties have the same values in part of base class.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        var thisType = GetType();
        var objType = obj.GetType();

        if (thisType != objType)
        {
            if (thisType.IsAssignableFrom(objType)
                || objType.IsAssignableFrom(thisType))
            {
                Logger.LogWarning($"Attempt to compare value object of a derived type detected: {thisType.FullName} vs {objType.FullName}");
            }
            return false;
        }

        return _properties.Value.All(p => PropertiesAreEqual(obj, p))
            && _fields.Value.All(f => FieldsAreEqual(obj, f));
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        unchecked   //allow overflow
        {
            var hash = 17;

            hash = _properties.Value
                .Aggregate(hash, (h, pi) => HashValue(h, pi.GetValue(this, null)!));

            hash = _fields.Value
                .Aggregate(hash, (h, fi) => HashValue(h, fi.GetValue(this)!));

            return hash;
        }
    }

    private bool PropertiesAreEqual(object obj, PropertyInfo p)
        => Equals(p.GetValue(this, null), p.GetValue(obj, null));

    private bool FieldsAreEqual(object obj, FieldInfo f)
        => Equals(f.GetValue(this), f.GetValue(obj));

    private static int HashValue(int seed, object value)
    {
        var currentHash = value != null
            ? value.GetHashCode()
            : 0;
        return seed * 23 + currentHash;
    }
}
