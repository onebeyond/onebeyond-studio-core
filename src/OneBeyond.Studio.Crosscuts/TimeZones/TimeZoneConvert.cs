using System;
using System.Diagnostics.CodeAnalysis;
using EnsureThat;
using TimeZoneConverter;

namespace OneBeyond.Studio.Crosscuts.TimeZones;

/// <summary>
/// </summary>
public static class TimeZoneConvert
{
    /// <summary>
    /// </summary>
    /// <param name="timeZoneId"></param>
    /// <param name="ianaId"></param>
    /// <returns></returns>
    public static bool TryToIanaId(string timeZoneId, [NotNullWhen(true)] out string? ianaId)
    {
        EnsureArg.IsNotNullOrWhiteSpace(timeZoneId, nameof(timeZoneId));

        if (TZConvert.TryIanaToWindows(timeZoneId, out _))
        {
            ianaId = timeZoneId;
            return true;
        }
        return TZConvert.TryWindowsToIana(timeZoneId, out ianaId);
    }

    /// <summary>
    /// </summary>
    /// <param name="timeZoneId"></param>
    /// <returns></returns>
    public static string ToIanaId(string timeZoneId)
        => TryToIanaId(timeZoneId, out var ianaId)
            ? ianaId
            : throw new InvalidTimeZoneException($"Unable to get time zone IANA ID for {timeZoneId}.");

    /// <summary>
    /// </summary>
    /// <param name="ianaOrWindowsId"></param>
    /// <param name="timeZoneInfo"></param>
    /// <returns></returns>
    public static bool TryGetTimeZoneInfo(string ianaOrWindowsId, [NotNullWhen(true)] out TimeZoneInfo? timeZoneInfo)
        => TZConvert.TryGetTimeZoneInfo(EnsureArg.IsNotEmptyOrWhiteSpace(ianaOrWindowsId, nameof(ianaOrWindowsId)), out timeZoneInfo);

    /// <summary>
    /// </summary>
    /// <param name="ianaOrWindowsId"></param>
    /// <returns></returns>
    public static TimeZoneInfo GetTimeZoneInfo(string ianaOrWindowsId)
        => TZConvert.GetTimeZoneInfo(EnsureArg.IsNotNullOrWhiteSpace(ianaOrWindowsId, nameof(ianaOrWindowsId)));
}
