using System.Diagnostics.CodeAnalysis;

namespace OneBeyond.Studio.Crosscuts.Strings;

/// <summary>
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// </summary>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? @this)
        => string.IsNullOrEmpty(@this);

    /// <summary>
    /// </summary>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? @this)
        => string.IsNullOrWhiteSpace(@this);

    /// <summary>
    /// </summary>
    public static string? ToFirstLetterLower(this string? @this)
    {
        if (@this.IsNullOrWhiteSpace())
        {
            return @this;
        }
        var charArray = @this.ToCharArray();
        charArray[0] = char.ToLower(charArray[0]);
        return new string(charArray);
    }
}
