using System;
using System.Collections.Generic;
using Ardalis.SmartEnum;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OneBeyond.Studio.DataAccess.EFCore.RelationalTypeMappings;

/// <summary>
/// </summary>
public abstract class SmartEnumRelationalTypeMapping : RelationalTypeMapping
{
    /// <summary>
    /// </summary>
    /// <param name="parameters"></param>
    protected SmartEnumRelationalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    /// </summary>
    protected static IReadOnlyDictionary<Type, string> ValueTypeToStoreTypeMap { get; } =
        new Dictionary<Type, string>
        {
                { typeof(string), "nvarchar(256)" },
                { typeof(int), "int" }
        };

    internal static bool IsValueTypeSupported(Type valueType)
        => ValueTypeToStoreTypeMap.ContainsKey(valueType);
}

/// <summary>
/// </summary>
/// <typeparam name="TSmartEnum"></typeparam>
/// <typeparam name="TValue"></typeparam>
public sealed class SmartEnumRelationalTypeMapping<TSmartEnum, TValue>
    : SmartEnumRelationalTypeMapping
    where TSmartEnum : SmartEnum<TSmartEnum, TValue>
    where TValue : IEquatable<TValue>, IComparable<TValue>
{
    internal SmartEnumRelationalTypeMapping()
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(TSmartEnum),
                    Converter),
                ValueTypeToStoreTypeMap[typeof(TValue)]))
    {
    }

    private SmartEnumRelationalTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    /// </summary>
    public static new readonly ValueConverter<TSmartEnum, TValue> Converter =
        new ValueConverter<TSmartEnum, TValue>(
            smartEnum => smartEnum.Value,
            smartEnumValue => SmartEnum<TSmartEnum, TValue>.FromValue(smartEnumValue));

    /// <summary>
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SmartEnumRelationalTypeMapping<TSmartEnum, TValue>(parameters);
}
