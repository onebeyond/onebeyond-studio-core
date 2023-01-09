using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ardalis.SmartEnum;
using EnsureThat;
using Microsoft.Extensions.Logging;
using MoreLinq;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Crosscuts.Reflection;
using OneBeyond.Studio.Crosscuts.Strings;

namespace OneBeyond.Studio.Application.SharedKernel.Specifications;

/// <summary>
/// </summary>
public static class FilterExpressionBuilder<T>
{
    private static readonly ILogger Logger = LogManager.CreateLogger(typeof(FilterExpressionBuilder<T>).Name);
    private static readonly Type DateTimeType = typeof(DateTime);
    private static readonly Type TType = typeof(T);
    private static readonly MethodInfo GetSmartEnumFromValueMethodInfo =
        typeof(FilterExpressionBuilder<T>).GetMethod(
            nameof(GetSmartEnumFromValue),
            BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo StringStartsWithMethodInfo =
        Reflector.MethodFrom((string s) => s.StartsWith(default(string)!));
    private static readonly MethodInfo StringEndsWithMethodInfo =
        Reflector.MethodFrom((string s) => s.EndsWith(default(string)!));
    private static readonly MethodInfo StringContainsMethodInfo =
        Reflector.MethodFrom((string s) => s.Contains(default(string)!));
    private static readonly MethodInfo StringEqualsMethodInfo =
        Reflector.MethodFrom((string s) => s.Equals(default));
    private static readonly MethodInfo StringToLowerMethodInfo =
        Reflector.MethodFrom((string s) => s.ToLower());
    private static readonly MethodInfo EnumerableAnyMethodInfo =
        Reflector.MethodFrom((IEnumerable<object> e) => e.Any((i) => true)).GetGenericMethodDefinition();

    private static readonly ConcurrentDictionary<string, PropertyInfo> PropertyDictionary =
        new ConcurrentDictionary<string, PropertyInfo>();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesDictionary =
        new ConcurrentDictionary<Type, PropertyInfo[]>();
    private static readonly ConcurrentDictionary<Type, Delegate> SmartEnumValueConverters =
        new ConcurrentDictionary<Type, Delegate>();

    /// <summary>
    /// <para>Builds lambda expression from key/value pairs where key is the name of the property and the value is the searched value</para>
    /// <para>For example from the folowing key/value pairs 'userID/1', 'customerID/2' will create the following lambda expression and every pair is joined with AND</para>
    /// <para>entity =&gt; entity.UserID = 1 &amp;&amp; entity.CustomerID = 2</para>
    /// </summary>
    public static Expression<Func<T, bool>>? Build(
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> query,
        CombineTypes combineType = CombineTypes.AND)
    {
        EnsureArg.IsNotNull(query, nameof(query));

        var cachedProperties = PropertiesDictionary.GetOrAdd(TType, (type) => type.GetProperties());

        var filterItems =
            (
                from keyval in query
                let propName = keyval.Key.Split(new char[] { '.' }, 2)[0]
                join prop in cachedProperties on propName.ToLower() equals prop.Name.ToLower() // Ignore query fields we've never heard of
                select new FilterItem
                {
                    Property = prop,
                    PathAndValues = keyval
                }
            ).ToArray();

        if (filterItems.Length == 0)
        {
            return null;
        }

        var parameterExpression = Expression.Parameter(TType, "entity");

        var expressions = EvaluateFilterItems(filterItems, parameterExpression);

        if (expressions.Count == 0)
        {
            return null;
        }

        var combinedExpression = CombineExpressions(expressions, combineType);

        return Expression.Lambda<Func<T, bool>>(combinedExpression, parameterExpression);
    }

    private static List<Expression> EvaluateFilterItems(
        IReadOnlyCollection<FilterItem> filterItems,
        ParameterExpression parameterExpression)
    {
        var expressions = new List<Expression>(filterItems.Count);

        foreach (var filterItem in filterItems)
        {
            var pathExpression = GetPathExpression(
                parameterExpression,
                filterItem.PathAndValues.Key) as Expression;

            var valueTokens = filterItem.PathAndValues.Value;

            var filterExpression = default(Expression);

            if (pathExpression.Type.IsCollectionOfT())
            {
                var itemType = pathExpression.Type.GenericTypeArguments[0];
                var itemExpression = Expression.Parameter(itemType, "item");
                var anyExpression = Expression.Lambda(
                    GetFilterExpression(itemExpression, valueTokens)!,
                    itemExpression);
                filterExpression = Expression.Call(
                    EnumerableAnyMethodInfo.MakeGenericMethod(itemType),
                    pathExpression,
                    anyExpression);
            }
            else
            {
                filterExpression = GetFilterExpression(pathExpression, valueTokens);
            }

            if (filterExpression is not null)
            {
                expressions.Add(filterExpression);
            }
        }

        return expressions;
    }

    private static Expression? GetFilterExpression(
        Expression pathExpression,
        IReadOnlyCollection<string> valueTokens)
    {
        return valueTokens
                .Assert(
                    (valueToken) => !valueToken.IsNullOrWhiteSpace(),
                    (valueToken) => new ArgumentException(
                        $"Unexpected null or white space value for path {pathExpression}"))
                .Select(
                    (valueToken) =>
                    {
                        return IsNotFunction(valueToken, out var argument)
                            ? Expression.Not(GetFilterExpression(pathExpression, argument!)!)
                            : GetFilterExpression(pathExpression, valueToken);
                    })
                .Where(
                    (filterExpression) => filterExpression is { })
                .Aggregate(
                    default(Expression),
                    (result, filterExpression) =>
                    {
                        return result is null
                            ? filterExpression
                            : Expression.OrElse(result, filterExpression!);
                    });
    }

    private static Expression? GetFilterExpression(Expression pathExpression, string valueToken)
    {
        return pathExpression switch
        {
            _ when pathExpression.Type.IsDateTime() && valueToken.Contains('&')
                => GetRangeExpressionForDate(pathExpression, valueToken.Split('&')),
            _ when pathExpression.Type.IsDateOnly() && valueToken.Contains('&')
                => GetRangeExpressionForDateOnly(pathExpression, valueToken.Split('&')),
            _ when pathExpression.Type.IsDateTimeOffset() && valueToken.Contains('&')
                => GetRangeExpressionForDateTimeOffset(pathExpression, valueToken.Split('&')),
            _ when pathExpression.Type.IsNumeric() && valueToken.Contains('&')
                => GetRangeExpressionForNumeric(pathExpression, valueToken.Split('&')),
            _ when pathExpression.Type.IsString()
                    && IsStringFunction(valueToken, out var function, out var argument)
                => GetFunctionExpressionForString(pathExpression, function!, argument!),
            _ when TryConvertFromString(valueToken, pathExpression.Type, out var value)
                => GetEqualsExpressionForType(
                    pathExpression,
                    Expression.Constant(value, pathExpression.Type)),
            _
                => default
        };
    }

    private static Expression CombineExpressions(List<Expression> expressions, CombineTypes type)
    {
        var curr = expressions[0];

        expressions.Remove(curr);

        switch (type)
        {
            case CombineTypes.AND:
                expressions.ForEach(ex => curr = Expression.AndAlso(curr, ex));
                break;
            case CombineTypes.OR:
                expressions.ForEach(ex => curr = Expression.OrElse(curr, ex));
                break;
            default:
                throw new NotSupportedException($"Compbine type {type} is not supprted.");
        }

        return curr;
    }

    private static MemberExpression GetPathExpression(
        Expression pathRootExpression,
        string path)
    {
        EnsureArg.IsNotNullOrWhiteSpace(path, nameof(path));

        var pathParts = path.Split(new char[] { '.' }, 2);

        if (pathParts.Length == 2) // Nested path
        {
            pathRootExpression = GetPathExpression(pathRootExpression, pathParts[0]);
            return GetPathExpression(pathRootExpression, pathParts[1]);
        }
        else
        {
            var propInfo = GetCachedProperty(pathRootExpression.Type, path);
            return Expression.PropertyOrField(pathRootExpression, propInfo.Name);
        }
    }

    private static PropertyInfo GetCachedProperty(Type type, string propertyName)
    {
        var propertyQualifiedName = type.AssemblyQualifiedName + propertyName;
        if (!PropertyDictionary.TryGetValue(propertyQualifiedName, out var info))
        {
            info = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)!;
            PropertyDictionary.TryAdd(propertyQualifiedName, info);
        }
        return info;
    }

    private static Expression GetEqualsExpressionForType(
        Expression propertyExpression,
        ConstantExpression someValue)
    {
        switch (propertyExpression.Type.IsEnum ? "Enum" : propertyExpression.Type.Name)
        {
            case "String":
                {
                    // entity => entity.[propName].Contains([someValue])
                    var method = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                    return Expression.Call(propertyExpression, method, someValue);
                }
            case "Nullable`1":
                {
                    // entity => entity.[propName].Value = [someValue]
                    var propertyValueExpression = Expression.PropertyOrField(propertyExpression, "Value");
                    var equalsExpression = GetEqualsExpressionForType(
                        propertyValueExpression,
                        Expression.Constant(someValue.Value));
                    return Expression.AndAlso(
                        Expression.NotEqual(propertyExpression, Expression.Constant(null)),
                        equalsExpression);
                }
            case "DateTime":
                {
                    // call .Date to truncate the time part
                    // entity => entity.[propName].Date = [someValue]
                    var datePropertyExp = Expression.PropertyOrField(propertyExpression, "Date");
                    return Expression.Equal(datePropertyExp, someValue);
                }
            default:
                // entity => entity.[propName] = [someValue]
                return Expression.Equal(propertyExpression, someValue);
        }
    }

    private static Expression GetRangeExpressionForDateOnly(
        Expression propertyExpression,
        string[] dateValuesAsString)
    {
        if (dateValuesAsString.Length != 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dateValuesAsString),
                "Value range requires 2 items separated by &");
        }

        var afterDate = default(DateOnly);
        if (!dateValuesAsString[0].IsNullOrWhiteSpace()
            && !DateOnly.TryParse(dateValuesAsString[0], out afterDate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(dateValuesAsString),
                $"Unable to parse value {dateValuesAsString[0]} as DateOnly");
        }

        var beforeDate = default(DateOnly);
        if (!dateValuesAsString[1].IsNullOrWhiteSpace()
            && !DateOnly.TryParse(dateValuesAsString[1], out beforeDate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(dateValuesAsString),
                $"Unable to parse value {dateValuesAsString[1]} as DateOnly");
        }

        var dateOnlyPropertyExpression = propertyExpression;
        if (dateOnlyPropertyExpression.Type.IsNullable())
        {
            dateOnlyPropertyExpression = Expression.Property(dateOnlyPropertyExpression, nameof(Nullable<int>.Value));
        }

        var rangeExpression = GetRangeExpression(
            dateOnlyPropertyExpression,
            afterDate == default
                ? default
                : Expression.Constant(afterDate),
            beforeDate == default
                ? default
                : Expression.Constant(beforeDate));

        return propertyExpression.Type.IsNullable()
            ? Expression.AndAlso(
                Expression.NotEqual(propertyExpression, Expression.Constant(null)),
                rangeExpression)
            : rangeExpression;
    }

    private static Expression GetRangeExpressionForDate(
        Expression propertyExpression,
        string[] dateValuesAsString)
    {
        if (dateValuesAsString.Length != 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dateValuesAsString),
                "Value range requires 2 items separated by &");
        }

        var afterDate = default(DateTime);
        if (!dateValuesAsString[0].IsNullOrWhiteSpace()
            && !DateTime.TryParse(dateValuesAsString[0], out afterDate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(dateValuesAsString),
                $"Unable to parse value {dateValuesAsString[0]} as DateTime");
        }
        afterDate = afterDate.Date;

        var beforeDate = default(DateTime);
        if (!dateValuesAsString[1].IsNullOrWhiteSpace()
            && !DateTime.TryParse(dateValuesAsString[1], out beforeDate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(dateValuesAsString),
                $"Unable to parse value {dateValuesAsString[1]} as DateTime");
        }
        beforeDate = beforeDate.Date;

        var datePropertyExpression = propertyExpression;
        if (datePropertyExpression.Type.IsNullable())
        {
            datePropertyExpression = Expression.Property(
                datePropertyExpression,
                nameof(Nullable<int>.Value));
        }
        datePropertyExpression = Expression.PropertyOrField(
            datePropertyExpression,
            nameof(DateTime.Date));

        var rangeExpression = GetRangeExpression(
            datePropertyExpression,
            afterDate == default
                ? default
                : Expression.Constant(afterDate),
            beforeDate == default
                ? default
                : Expression.Constant(beforeDate));

        return propertyExpression.Type.IsNullable()
            ? Expression.AndAlso(
                Expression.NotEqual(propertyExpression, Expression.Constant(null)),
                rangeExpression)
            : rangeExpression;
    }

    private static Expression GetRangeExpressionForDateTimeOffset(
        Expression propertyExpression,
        string[] dateTimeOffsetValuesAsString)
    {
        if (dateTimeOffsetValuesAsString.Length != 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dateTimeOffsetValuesAsString),
                "Value range requires 2 items separated by &");
        }

        var afterDate = default(DateTimeOffset);
        if (!dateTimeOffsetValuesAsString[0].IsNullOrWhiteSpace()
            && !DateTimeOffset.TryParse(dateTimeOffsetValuesAsString[0], out afterDate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(dateTimeOffsetValuesAsString),
                $"Unable to parse value {dateTimeOffsetValuesAsString[0]} as DateTimeOffset");
        }

        var beforeDate = default(DateTimeOffset);
        if (!dateTimeOffsetValuesAsString[1].IsNullOrWhiteSpace()
            && !DateTimeOffset.TryParse(dateTimeOffsetValuesAsString[1], out beforeDate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(dateTimeOffsetValuesAsString),
                $"Unable to parse value {dateTimeOffsetValuesAsString[1]} as DateTimeOffset");
        }

        var datePropertyExpression = propertyExpression;
        if (datePropertyExpression.Type.IsNullable())
        {
            datePropertyExpression = Expression.Property(datePropertyExpression, nameof(Nullable<int>.Value));
        }

        var rangeExpression = GetRangeExpression(
            datePropertyExpression,
            afterDate == default
                ? default
                : Expression.Constant(afterDate),
            beforeDate == default
                ? default
                : Expression.Constant(beforeDate));

        return propertyExpression.Type.IsNullable()
            ? Expression.AndAlso(
                Expression.NotEqual(propertyExpression, Expression.Constant(null)),
                rangeExpression)
            : rangeExpression;
    }

    private static Expression GetRangeExpressionForNumeric(
        Expression propertyExpression,
        string[] numericValuesAsString)
    {
        if (numericValuesAsString.Length != 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numericValuesAsString),
                "Value range requires 2 items");
        }

        var gteNumeric = default(object);
        if (!numericValuesAsString[0].IsNullOrWhiteSpace())
        {
            gteNumeric = Convert.ChangeType(
                    numericValuesAsString[0],
                    Nullable.GetUnderlyingType(propertyExpression.Type) ?? propertyExpression.Type)
                ?? throw new ArgumentOutOfRangeException(
                    nameof(numericValuesAsString),
                    $"Unable to parse value {numericValuesAsString[0]} as {propertyExpression.Type.Name}");
        }

        var lteNumeric = default(object);
        if (!numericValuesAsString[1].IsNullOrWhiteSpace())
        {
            lteNumeric = Convert.ChangeType(
                    numericValuesAsString[1],
                    Nullable.GetUnderlyingType(propertyExpression.Type) ?? propertyExpression.Type)
                ?? throw new ArgumentOutOfRangeException(
                    nameof(numericValuesAsString),
                    $"Unable to parse value {numericValuesAsString[1]} as {propertyExpression.Type.Name}");
        }

        return GetRangeExpression(
            propertyExpression,
            gteNumeric is null
                ? default
                : Expression.Constant(gteNumeric),
            lteNumeric is null
                ? default
                : Expression.Constant(lteNumeric));
    }

    private static Expression GetRangeExpression(
        Expression valueExpresion,
        Expression? gteValueExpression,
        Expression? lteValueExpression)
    {
        EnsureArg.IsFalse(gteValueExpression is null && lteValueExpression is null);

        var valueNotNullExpression = valueExpresion.Type.IsNullable()
            ? Expression.NotEqual(valueExpresion, Expression.Constant(null))
            : default;

        valueExpresion = valueExpresion.Type.IsNullable()
            ? Expression.Property(valueExpresion, nameof(Nullable<int>.Value))
            : valueExpresion;

        var gteExpression = gteValueExpression is null
            ? default
            : Expression.GreaterThanOrEqual(valueExpresion, gteValueExpression);

        var lteExpression = lteValueExpression is null
            ? default
            : Expression.LessThanOrEqual(valueExpresion, lteValueExpression);

        Expression? expression;
        if (gteExpression is null)
        {
            expression = lteExpression;
        }
        else if (lteExpression is null)
        {
            expression = gteExpression;
        }
        else
        {
            expression = Expression.AndAlso(gteExpression, lteExpression);
        }
        return valueNotNullExpression is null
            ? expression!
            : Expression.AndAlso(valueNotNullExpression, expression!);
    }

    private static Expression GetFunctionExpressionForString(
        Expression pathExpression,
        MethodInfo function,
        string argument)
        => Expression.Call(
            Expression.Call(pathExpression, StringToLowerMethodInfo),
            function,
            Expression.Constant(argument.ToLower()));

    private static bool TryConvertFromString(string valueToken, Type type, out object? value)
    {
        valueToken = TrimEqualsFunction(valueToken);
        try
        {
            value = null;

            if (type.IsEnum)
            {
                return TryConvertEnumFromString(type, valueToken, out value);
            }
            else if (Nullable.GetUnderlyingType(type)?.IsEnum == true)
            {
                return TryConvertEnumFromString(Nullable.GetUnderlyingType(type)!, valueToken, out value);
            }
            else if (type == typeof(bool)
                || Nullable.GetUnderlyingType(type) == typeof(bool))
            {
                value = TryConvertBoolFromString(valueToken, value);
            }
            else if (type == typeof(DateTime)
                || Nullable.GetUnderlyingType(type) == typeof(DateTime))
            {
                return TryConvertDateTimeFromString(valueToken, out value);
            }
            else if (type == typeof(DateTimeOffset)
                || Nullable.GetUnderlyingType(type) == typeof(DateTimeOffset))
            {
                return TryConvertDateTimeOffsetFromString(valueToken, out value);
            }
            else if (type == typeof(DateOnly)
                || Nullable.GetUnderlyingType(type) == typeof(DateOnly))
            {
                return TryConvertDateOnlyFromString(valueToken, out value);
            }
            else if (type == typeof(Guid)
                || Nullable.GetUnderlyingType(type) == typeof(Guid))
            {
                return TryConvertGuidFromString(valueToken, out value);
            }
            else if (type.IsNullable())
            {
                var innerType = Nullable.GetUnderlyingType(type)!;

                value = Convert.ChangeType(valueToken, innerType);
            }
            else if (type.IsSmartEnum(out var smartEnumGenericArguments))
            {
                var smartEnumValueConverter = SmartEnumValueConverters.GetOrAdd(
                    type,
                    (_) =>
                    {
                        var getSmartEnumFromValueMethodInfo =
                            GetSmartEnumFromValueMethodInfo.MakeGenericMethod(smartEnumGenericArguments);
                        var smartEnumValueParameter = Expression.Parameter(smartEnumGenericArguments[1]);
                        var getSmartEnumFromValueLambda = Expression.Lambda(
                            Expression.Call(getSmartEnumFromValueMethodInfo, smartEnumValueParameter),
                            smartEnumValueParameter);
                        return getSmartEnumFromValueLambda.Compile();
                    });
                value = Convert.ChangeType(valueToken, smartEnumGenericArguments[1]);
                value = smartEnumValueConverter.DynamicInvoke(value);
            }
            else
            {
                valueToken = valueToken.Trim().ToLower();
                value = valueToken.StartsWith('"') && valueToken.EndsWith('"')
                    ? Convert.ChangeType(valueToken[1..^1], type)
                    : Convert.ChangeType(valueToken, type);
            }

            return true;
        }
        catch (Exception exception)
        when (!exception.IsCritical())
        {
            Logger.LogError(exception, "TryConvertFromString error");
            value = null;
            return false;
        }
    }

    private static bool TryConvertEnumFromString(Type type, string valueToConvert, out object? value)
    {
        try
        {
            value = Enum.Parse(type, valueToConvert);
            return true;
        }
        catch (Exception exception)
        when (!exception.IsCritical())
        {
            value = null;
            return false;
        }
    }

    private static bool TryConvertGuidFromString(string valueToConvert, out object? value)
    {
        if (Guid.TryParse(valueToConvert, out var g))
        {
            value = g;
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    private static bool TryConvertDateTimeFromString(string valueToConvert, out object? value)
    {
        if (DateTime.TryParse(valueToConvert, out var dt))
        {
            value = dt;
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    private static bool TryConvertDateTimeOffsetFromString(string valueToConvert, out object? value)
    {
        if (DateTimeOffset.TryParse(valueToConvert, out var dt))
        {
            value = dt;
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    private static bool TryConvertDateOnlyFromString(string valueToConvert, out object? value)
    {
        if (DateOnly.TryParse(valueToConvert, out var dateOnly))
        {
            value = dateOnly;
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    private static object? TryConvertBoolFromString(string valueToConvert, object? value)
    {
        var lowerPropVal = valueToConvert.ToLower();
        if (lowerPropVal == "yes" || lowerPropVal == "true")
        {
            lowerPropVal = "1";
        }

        if (lowerPropVal == "no" || lowerPropVal == "false")
        {
            lowerPropVal = "0";
        }

        if (int.TryParse(lowerPropVal, out var intVal))
        {
            value = Convert.ToBoolean(intVal);
        }

        return value;
    }

    private static bool IsStringFunction(string valueToken, out MethodInfo? function, out string? argument)
    {
        valueToken = valueToken.Trim();
        function = default;
        argument = default;
        if (!valueToken.EndsWith(")"))
        {
            return false;
        }
        const string StartsWithPrefix = nameof(string.StartsWith) + "(";
        const string EndsWithPrefix = nameof(string.EndsWith) + "(";
        const string ContainsPrefix = nameof(string.Contains) + "(";
        const string EqualsPrefix = nameof(string.Equals) + "(";
        if (valueToken.StartsWith(StartsWithPrefix, StringComparison.OrdinalIgnoreCase))
        {
            function = StringStartsWithMethodInfo;
            argument = valueToken[StartsWithPrefix.Length..^1];
            return true;
        }
        else if (valueToken.StartsWith(EndsWithPrefix, StringComparison.OrdinalIgnoreCase))
        {
            function = StringEndsWithMethodInfo;
            argument = valueToken[EndsWithPrefix.Length..^1];
            return true;
        }
        else if (valueToken.StartsWith(ContainsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            function = StringContainsMethodInfo;
            argument = valueToken[ContainsPrefix.Length..^1];
            return true;
        }
        else if (valueToken.StartsWith(EqualsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            function = StringEqualsMethodInfo;
            argument = valueToken[EqualsPrefix.Length..^1];
            return true;
        }
        return false;
    }

    private static bool IsNotFunction(string valueToken, out string? argument)
    {
        valueToken = valueToken.Trim();
        argument = default;
        if (!valueToken.EndsWith(")"))
        {
            return false;
        }
        const string NotPrefix = "not(";
        if (valueToken.StartsWith(NotPrefix, StringComparison.OrdinalIgnoreCase))
        {
            argument = valueToken[NotPrefix.Length..^1];
            return true;
        }
        return false;
    }

    private static string TrimEqualsFunction(string valueToken)
    {
        valueToken = valueToken.Trim();
        if (!valueToken.EndsWith(")"))
        {
            return valueToken;
        }
        const string EqualsPrefix = nameof(string.Equals) + "(";
        return valueToken.StartsWith(EqualsPrefix, StringComparison.OrdinalIgnoreCase)
            ? valueToken[EqualsPrefix.Length..^1]
            : valueToken;
    }

    private static TSmartEnum GetSmartEnumFromValue<TSmartEnum, TValue>(TValue value)
        where TSmartEnum : SmartEnum<TSmartEnum, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue>
        => SmartEnum<TSmartEnum, TValue>.FromValue(value);

    /// <summary>
    /// </summary>
    public enum CombineTypes
    {
        /// <summary>
        /// </summary>
        AND = 1,
        /// <summary>
        /// </summary>
        OR = 2
    }

    private sealed class FilterItem
    {
        public PropertyInfo Property { get; set; } = null!;
        public KeyValuePair<string, IReadOnlyCollection<string>> PathAndValues { get; set; }
    }
}
