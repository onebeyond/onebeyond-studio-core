namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OtherValidators;

/// <summary>
/// </summary>
public sealed class TxtValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public TxtValidator()
        : base(
              "text/plain",
              new[] { ".txt" },
              "")
    {
    }
}
