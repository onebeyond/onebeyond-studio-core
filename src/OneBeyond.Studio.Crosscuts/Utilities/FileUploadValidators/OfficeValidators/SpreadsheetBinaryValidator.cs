namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class SpreadsheetBinaryValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public SpreadsheetBinaryValidator()
        : base(
            "application/vnd.ms-excel.sheet.binary.macroEnabled.12",
            new[] { ".xlsb" },
            "50-4B-03-04-14-00-06-00")
    {
    }
}
