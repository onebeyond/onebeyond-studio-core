namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class WordMacroValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public WordMacroValidator()
        : base(
            "application/vnd.ms-word.document.macroEnabled.12",
            new[] { ".docm" },
            "50-4B-03-04-14-00-06-00")
    {
    }
}
