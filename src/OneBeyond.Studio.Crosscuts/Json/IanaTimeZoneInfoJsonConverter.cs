using System;
using EnsureThat;
using Newtonsoft.Json;
using OneBeyond.Studio.Crosscuts.TimeZones;

namespace OneBeyond.Studio.Crosscuts.Json;

/// <summary>
/// Converts a TimeZoneInfo to and from IANA.
/// </summary>
public sealed class IanaTimeZoneInfoJsonConverter : JsonConverter<TimeZoneInfo>
{
    /// <summary>
    /// Converts a TimeZoneInfo to IANA ID.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson(
        JsonWriter writer,
        TimeZoneInfo? value,
        JsonSerializer serializer)
    {
        EnsureArg.IsNotNull(writer, nameof(writer));

        if (value is not null)
        {
            writer.WriteValue(TimeZoneConvert.ToIanaId(value.Id));
        }
        else
        {
            writer.WriteNull();
        }
    }

    /// <summary>
    /// Converts IANA ID as string to TimeZoneInfo.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override TimeZoneInfo? ReadJson(
        JsonReader reader,
        Type objectType,
        TimeZoneInfo? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        EnsureArg.IsNotNull(reader, nameof(reader));

        return reader.Value is not string valueAsString
            ? default
            : TimeZoneConvert.GetTimeZoneInfo(valueAsString);
    }

}
