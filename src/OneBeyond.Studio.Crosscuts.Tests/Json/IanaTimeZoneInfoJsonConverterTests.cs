using System;
using Newtonsoft.Json;
using OneBeyond.Studio.Crosscuts.Json;
using OneBeyond.Studio.Crosscuts.TimeZones;
using Xunit;

namespace OneBeyond.Studio.Crosscuts.Tests.Json;

public sealed class IanaTimeZoneInfoJsonConverterTests
{
    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Eastern Standard Time")]
    public void When_IanaOrWindowsTimeZone_Then_ConvertsToIana(string timeZoneId)
    {
        var timeZone = TimeZoneConvert.GetTimeZoneInfo(timeZoneId);
        var json = JsonConvert.SerializeObject(timeZone, new IanaTimeZoneInfoJsonConverter());

        Assert.Equal("America/New_York", json.Trim('"'), true);
    }

    [Theory]
    [InlineData("\"America/New_York\"")]
    [InlineData("\"Eastern Standard Time\"")]
    public void When_IanaOrWindowsId_Then_ConvertsToTimeZone(string timeZoneId)
    {
        var timeZone = JsonConvert.DeserializeObject<TimeZoneInfo>(timeZoneId, new IanaTimeZoneInfoJsonConverter())!;
        var timeZoneNewYork = TimeZoneConvert.GetTimeZoneInfo("America/New_York");

        Assert.Equal(timeZone, timeZoneNewYork);
    }

    [Fact]
    public void When_NullTimeZone_Then_ReturnsNull()
    {
        var json = JsonConvert.SerializeObject(null, new IanaTimeZoneInfoJsonConverter());

        Assert.Equal("null", json);
    }
}
