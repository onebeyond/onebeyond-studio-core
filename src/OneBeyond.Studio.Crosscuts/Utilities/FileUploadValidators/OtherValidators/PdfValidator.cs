namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OtherValidators;

/// <summary>
/// </summary>
public sealed class PdfValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public PdfValidator()
        : base(
              "application/pdf",
              new[] { ".pdf" },
              "25-50-44-46")
    {
    }
}
