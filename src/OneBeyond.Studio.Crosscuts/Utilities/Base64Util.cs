using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace OneBeyond.Studio.Crosscuts.Utilities;

/// <summary>
/// </summary>
public static class Base64Util
{
    /// <summary>Encode a plain text to Base64</summary>
    /// <param name="plainText">The text to encode</param>
    /// <returns></returns>
    public static string Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

        return Convert.ToBase64String(plainTextBytes);
    }

    /// <summary>Decode a Base64 text back to plain text.</summary>
    /// <param name="base64EncodedData">The Base64 text to decode</param>
    /// <returns></returns>
    public static string Decode(string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

        return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    /// <summary>Check if a String is base64encoded</summary>
    /// <param name="base64String"></param>
    /// <returns></returns>
    public static bool IsBase64(this string base64String)
    {
        base64String = base64String.Trim();

        return base64String.Length % 4 == 0 &&
               Regex.IsMatch(base64String, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
    }
}
