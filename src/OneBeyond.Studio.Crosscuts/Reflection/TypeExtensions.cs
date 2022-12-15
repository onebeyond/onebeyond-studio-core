using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;

namespace OneBeyond.Studio.Crosscuts.Reflection;

/// <summary>
/// Some useful extensions for <see cref="Type"/>
/// </summary>
public static class TypeExtensions
{
    private static readonly Type NullableTypeDefinition = typeof(Nullable<>);
    private static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);
    private static readonly Type IEnumerableType = typeof(IEnumerable);
    private static readonly Type IEnumerableOfTType = typeof(IEnumerable<>);

    /// <summary>
    /// Checks whether the specified type represents a numeric value
    /// </summary>
    public static bool IsNumeric(this Type type)
    {
        EnsureArg.IsNotNull(type, nameof(type));

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.Single:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return true;
            case TypeCode.Object:
                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == NullableTypeDefinition)
                {
                    return Nullable.GetUnderlyingType(type)!.IsNumeric();
                }
                return false;
            default:
                return false;
        }
    }

    /// <summary>
    /// Checks whether the specified type represents a string value
    /// </summary>
    public static bool IsString(this Type type)
    {
        EnsureArg.IsNotNull(type, nameof(type));

        return Type.GetTypeCode(type) == TypeCode.String;
    }

    /// <summary>
    /// Checks whether the specified type represents any sort of collection whereas
    /// string value is not treated as one
    /// </summary>
    public static bool IsCollection(this Type type)
    {
        EnsureArg.IsNotNull(type, nameof(type));

        return !type.IsString() && IEnumerableType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Checks whether the specified type represents any sort of collection whereas
    /// string value is not treated as one
    /// </summary>
    public static bool IsCollectionOfT(this Type type)
    {
        EnsureArg.IsNotNull(type, nameof(type));

        return type.IsCollection()
            && (type.IsGenericType && type.GetGenericTypeDefinition() == IEnumerableOfTType
                || type.GetInterfaces()
                    .Any((@interface) => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == IEnumerableOfTType));
    }

    /// <summary>
    /// Checks whether the specified type represents a DateTime/DateTime? value
    /// </summary>
    public static bool IsDateTime(this Type type)
    {
        EnsureArg.IsNotNull(type, nameof(type));

        return Type.GetTypeCode(type) switch
        {
            TypeCode.DateTime
                => true,
            TypeCode.Object
                => type.IsNullable()
                    && Type.GetTypeCode(Nullable.GetUnderlyingType(type)) == TypeCode.DateTime,
            _
                => false
        };
    }

    /// <summary>
    /// Checks whether the specified type represents a DateOnly/DateOnly? value
    /// </summary>
    public static bool IsDateOnly(this Type type)
    {
        EnsureArg.IsNotNull(type, nameof(type));

        return type == typeof(DateOnly)
            || type == typeof(DateOnly?);
    }

    /// <summary>
    /// Checks whether the specified type represents a DateTimeOffset/DateTimeOffset? value
    /// </summary>
    public static bool IsDateTimeOffset(this Type type)
    {
        EnsureArg.IsNotNull(type, nameof(type));

        return type switch
        {
            _ when type == DateTimeOffsetType
                => true,
            _ when Nullable.GetUnderlyingType(type) == DateTimeOffsetType
                => true,
            _
                => false
        };
    }

    /// <summary>
    /// Checks whether the specified type is nullable
    /// </summary>
    public static bool IsNullable(this Type type)
    {
        EnsureArg.IsNotNull(type, nameof(type));

        return type.IsGenericType
            && type.GetGenericTypeDefinition() == NullableTypeDefinition;
    }
}
