namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class WordXValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public WordXValidator()
        : base(
              "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
              new[] { ".docx" },
              "50-4B-03-04")
    {
    }
}
