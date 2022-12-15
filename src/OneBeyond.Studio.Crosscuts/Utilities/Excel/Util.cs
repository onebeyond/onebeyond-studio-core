using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OneBeyond.Studio.Crosscuts.Utilities.Excel;

/// <summary>
/// </summary>
public static class Util
{
    /// <summary>
    /// </summary>
    /// <param name="data"></param>
    /// <param name="jArray"></param>
    /// <returns></returns>
    public static bool TryConvertToJArray(object data, out JArray? jArray)
    {
        try
        {
            jArray = ConvertToJArray(data);
            return true;
        }
        catch
        {
            jArray = null;
            return false;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static JArray ConvertToJArray(object data)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            return JsonConvert.DeserializeObject<JArray>(json)!;
        }
        catch (Exception e)
        {
            throw new ArgumentException("data cannot be converted to JArray", e);
        }
    }
}
