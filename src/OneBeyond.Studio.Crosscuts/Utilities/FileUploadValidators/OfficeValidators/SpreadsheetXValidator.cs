namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class SpreadsheetXValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public SpreadsheetXValidator()
        : base(
              "application/vnd.openxmlformats-officedocument.spreadsheetml.document",
              new[] { ".xlsx" },
              "50-4B-03-04-14-00-06-00")
    {
    }
}
