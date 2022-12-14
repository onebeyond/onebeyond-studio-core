using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FluentAssertions;
using OneBeyond.Studio.Crosscuts.Utilities.Excel;
using Xunit;

namespace OneBeyond.Studio.Crosscuts.Tests;

public sealed class ExcelGeneratorTest : IDisposable
{
    public ExcelGeneratorTest() => _exportableList = new List<EntityTestWithAllTypes> {
                new EntityTestWithAllTypes {
                        IntID                 = 1,
                        StringName            = "Alfredo",
                        BooleanOk             = true,
                        DateTimeToday         = DateTime.Today,
                        DateTimeNow           = DateTime.Now,
                        DoubleValue           = 1.10,
                        IntIDNullable         = null,
                        StringNameNullable    = null,
                        DateTimeTodayNullable = null,
                        DateTimeNowNullable   = null,
                        DoubleValueNullable   = null,
                        BooleanOkNullable     = null
                },
                new EntityTestWithAllTypes {
                        IntID                 = 1,
                        StringName            = "Cristiano",
                        BooleanOk             = true,
                        DateTimeToday         = DateTime.Today,
                        DateTimeNow           = DateTime.Now,
                        DoubleValue           = 1.10,
                        IntIDNullable         = null,
                        StringNameNullable    = null,
                        DateTimeTodayNullable = null,
                        DateTimeNowNullable   = null,
                        DoubleValueNullable   = null,
                        BooleanOkNullable     = null
                },
                new EntityTestWithAllTypes {
                        IntID                 = 1,
                        StringName            = "Camilo",
                        BooleanOk             = false,
                        DateTimeToday         = DateTime.Today,
                        DateTimeNow           = DateTime.Now,
                        DoubleValue           = 100,
                        IntIDNullable         = null,
                        StringNameNullable    = null,
                        DateTimeTodayNullable = null,
                        DateTimeNowNullable   = null,
                        DoubleValueNullable   = null,
                        BooleanOkNullable     = null
                }
        };

    public void Dispose() => _exportableList = null;

    private List<EntityTestWithAllTypes>? _exportableList;

    private class EntityTestWithAllTypes
    {
        public int IntID { get; set; }
        public string? StringName { get; set; }
        public bool BooleanOk { get; set; }
        public DateTime DateTimeToday { get; set; }
        public DateTime DateTimeNow { get; set; }
        public double DoubleValue { get; set; }
        public int? IntIDNullable { get; set; }
        public string? StringNameNullable { get; set; }
        public bool? BooleanOkNullable { get; set; }
        public DateTime? DateTimeTodayNullable { get; set; }
        public DateTime? DateTimeNowNullable { get; set; }
        public double? DoubleValueNullable { get; set; }
    }

    //Ignored to not run on the server, but should be run on local machines to check the consistency of the Generated Excel
    //[Fact]
    public void SaveFileToCheck()
    {
        var jsonData = Util.ConvertToJArray(_exportableList!);
        var excelGenerator = new ExcelGenerator();
        var bytes = excelGenerator.GetExcelBytes(jsonData, "Sheet1");
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var f = new FileInfo(path);
        var d = f.Directory;
        File.WriteAllBytes($"{d}\\excel.xlsx", bytes);
    }

    [Fact]
    public void TestExcelGeneration()
    {
        var jsonData = Util.ConvertToJArray(_exportableList!);
        var excelGenerator = new ExcelGenerator();
        excelGenerator.Invoking(g => g.GetExcelBytes(jsonData, "Sheet1")).Should().NotThrow<Exception>();
    }
}
