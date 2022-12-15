using Newtonsoft.Json.Linq;

namespace OneBeyond.Studio.Crosscuts.Utilities.Excel;

/// <summary>
/// </summary>
public interface IExcelExport
{
    /// <summary>
    /// </summary>
    /// <param name="jsonData"></param>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    byte[] GetExcelBytes(JArray jsonData, string sheetName);
}
