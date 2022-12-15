using System;

namespace OneBeyond.Studio.Crosscuts.DateTimes;

/// <summary>
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// </summary>
    public static string ToInvertedTicks(this DateTime dateTime)
        => string.Format("{0:D19}", DateTime.MaxValue.Ticks - dateTime.Ticks);

    /// <summary>
    /// </summary>
    public static string ToInvertedTicks(this DateTimeOffset dateTime)
        => string.Format("{0:D19}", DateTime.MaxValue.Ticks - dateTime.Ticks);

    /// <summary>
    /// </summary>
    public static DateTime FromInvertedTicks(string invertedTicks)
        => new DateTime(DateTime.MaxValue.Ticks - long.Parse(invertedTicks));
}
