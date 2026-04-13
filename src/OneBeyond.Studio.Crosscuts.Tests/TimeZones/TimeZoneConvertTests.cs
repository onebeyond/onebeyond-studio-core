using OneBeyond.Studio.Crosscuts.TimeZones;
using Xunit;

namespace OneBeyond.Studio.Crosscuts.Tests.TimeZones;


public sealed class TimeZoneConvertTests
{
    [Fact]
    public void TestTheSameIdIsReturnedForIanaTimeZoneInfo()
    {
        var ianaId = TimeZoneConvert.ToIanaId("America/New_York");

        Assert.Equal("America/New_York", ianaId);
    }

    [Fact]
    public void TestIanaIdIsReturnedForWindowsTimeZoneInfo()
    {
        var ianaId = TimeZoneConvert.ToIanaId("Tokyo Standard Time");

        Assert.Equal("Asia/Tokyo", ianaId);
    }

    [Fact]    
    public void TestItThrowsForUnknownTimeZoneInfo()
    {
        Assert.Throws<InvalidTimeZoneException>(() => TimeZoneConvert.ToIanaId("Custom Time"));       
    }
}

