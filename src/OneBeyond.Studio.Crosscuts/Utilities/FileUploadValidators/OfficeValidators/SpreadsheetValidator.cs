namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class SpreadsheetValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public SpreadsheetValidator()
        : base(
              "application/msexcel",
              new[] { ".xls" },
              "D0-CF-11-E0-A1-B1-1A-E1")
    {
    }
}
