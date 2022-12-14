using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OneBeyond.Studio.Crosscuts.Utilities.Enums;

/// <summary>
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<EnumValue> GetEnumValueList(this Type type)
    {
        if (!type.IsEnum)
        {
            throw new ArgumentException($"EnumerationValue must be of Enum type. Type received {type.Name}.", nameof(type));
        }

        var items = Enum.GetValues(type).Cast<object>()
            .Select(val => new EnumValue
            {
                Name = GetDisplayDescriptionValue(type, val),
                Id = (int)val
            });

        return items;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerationValue"></param>
    /// <returns></returns>
    public static string GetDisplayDescriptionValue<T>(this T enumerationValue)
        where T : struct
    {
        var type = enumerationValue.GetType();
        return type.IsEnum
            ? GetDisplayDescriptionValue(type, enumerationValue)
            : throw new ArgumentException($"EnumerationValue must be of Enum type. Type received {type.Name}.", nameof(enumerationValue));
    }

    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetDisplayDescriptionValue(Type type, object value)
    {
        var fieldInfo = type.GetField(value.ToString()!)!;

        return fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] descriptionAttributes
            ? descriptionAttributes.Length > 0 ? descriptionAttributes[0].Name! : value.ToString()!
            : string.Empty;
    }

    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetDisplayGroupValue(Type type, object value)
    {
        var fieldInfo = type.GetField(value.ToString()!)!;

        return fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] descriptionAttributes
            ? descriptionAttributes.Length > 0 ? descriptionAttributes[0].GroupName! : value.ToString()!
            : string.Empty;
    }
}
