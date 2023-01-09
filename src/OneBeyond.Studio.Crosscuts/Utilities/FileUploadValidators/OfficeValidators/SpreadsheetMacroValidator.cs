namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class SpreadsheetMacroValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public SpreadsheetMacroValidator() : base(
        "application/vnd.ms-excel.sheet.macroEnabled.12",
        new[] { ".xlsm" },
        "50-4B-03-04-14-00-06-00")
    {
    }
}
