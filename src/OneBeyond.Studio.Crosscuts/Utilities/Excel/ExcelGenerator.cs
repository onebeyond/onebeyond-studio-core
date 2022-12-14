using System.Linq;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;

#nullable disable

namespace OneBeyond.Studio.Crosscuts.Utilities.Excel;

/// <summary>Utility class to generate and excel</summary>
public class ExcelGenerator : IExcelExport
{
    /// <summary>
    /// </summary>
    /// <param name="jsonData"></param>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    public byte[] GetExcelBytes(JArray jsonData, string sheetName)
    {
        using (var p = new ExcelPackage())
        {
            var ws = p.Workbook.Worksheets.Add(sheetName);

            var cols = GetColumnsAndIncludeTableHeaders(ws, jsonData);

            for (var r = 0; r < jsonData.Count; r++)
            {
                for (var i = 0; i < cols.Length; i++)
                {
                    var value = jsonData[r][cols[i]].Value<string>();

                    if (null != value)
                    {
                        ws.Cells[r + 2, i + 1].Value = value;
                    }
                }
            }

            var bytes = p.GetAsByteArray();

            return bytes;
        }
    }

    private string[] GetColumnsAndIncludeTableHeaders(ExcelWorksheet ws, JArray data)
    {
        var firstRow = data.Children<JObject>().First();
        var propNames = firstRow.Properties().Select(prop => prop.Name).ToArray();
        var colIdx = 1;

        foreach (var prop in propNames)
        {
            ws.InsertColumn(colIdx, 1);
            ws.Cells[1, colIdx].Value = prop;
            colIdx++;
        }

        return propNames;
    }
}
