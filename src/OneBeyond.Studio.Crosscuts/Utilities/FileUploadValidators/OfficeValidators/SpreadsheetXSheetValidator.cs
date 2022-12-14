namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class SpreadsheetXSheetValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public SpreadsheetXSheetValidator()
        : base(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            new[] { ".xlsx" },
            "50-4B-03-04-14-00-06-00")
    {
    }
}
