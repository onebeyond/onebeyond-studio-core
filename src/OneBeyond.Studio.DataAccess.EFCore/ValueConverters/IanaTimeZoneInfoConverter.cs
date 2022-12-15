using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OneBeyond.Studio.Crosscuts.TimeZones;

namespace OneBeyond.Studio.DataAccess.EFCore.ValueConverters;

/// <summary>
/// </summary>
public static class IanaTimeZoneInfoConverter
{
    /// <summary>
    /// </summary>
    public static ValueConverter<TimeZoneInfo, string> Instance { get; } =
        new(
            (timeZoneInfo) => TimeZoneConvert.ToIanaId(timeZoneInfo.Id),
            (timeZoneId) => TimeZoneConvert.GetTimeZoneInfo(timeZoneId),
            new ConverterMappingHints(size: 128));
}
