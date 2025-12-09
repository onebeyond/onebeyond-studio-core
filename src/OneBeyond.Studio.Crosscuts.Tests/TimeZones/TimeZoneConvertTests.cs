using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Crosscuts.TimeZones;

namespace OneBeyond.Studio.Crosscuts.Tests.TimeZones;

[TestClass]
public sealed class TimeZoneConvertTests
{
    [TestMethod]
    public void TestTheSameIdIsReturnedForIanaTimeZoneInfo()
    {
        var ianaId = TimeZoneConvert.ToIanaId("America/New_York");

        Assert.AreEqual("America/New_York", ianaId);
    }

    [TestMethod]
    public void TestIanaIdIsReturnedForWindowsTimeZoneInfo()
    {
        var ianaId = TimeZoneConvert.ToIanaId("Tokyo Standard Time");

        Assert.AreEqual("Asia/Tokyo", ianaId);
    }

    [TestMethod]    
    public void TestItThrowsForUnknownTimeZoneInfo()
    {
        Assert.Throws<InvalidTimeZoneException>(() => TimeZoneConvert.ToIanaId("Custom Time"));       
    }
}
