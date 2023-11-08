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
              new[] { ".doc", ".rtf" },
              new[] {
                  "D0-CF-11-E0-A1-B1-1A-E1",
                  // rtf will have different signatures depending on which program created them
                  "50-4B-03-04-14",
                  "7B-5C-72-74-66"
              })
    {
    }
}
