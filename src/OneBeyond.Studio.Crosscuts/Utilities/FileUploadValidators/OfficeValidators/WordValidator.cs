namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class WordValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public WordValidator()
        : base(
              "application/msword",
              new[] { ".doc" },
              "D0-CF-11-E0-A1-B1-1A-E1")
    {
    }
}
