using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class SpreadsheetXlsValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public SpreadsheetXlsValidator()
        : base(
            "application/vnd.ms-excel",
            new[] { ".xls" },
            new[]
            {
                    "09-08-10-00-00-06-05-00",
                    "FD-FF-FF-FF-20-00-00-00",
                    "D0-CF-11-E0-A1-B1-1A-E1",
                    "FD-FF-FF-FF-04",
            })
    {
    }
}
