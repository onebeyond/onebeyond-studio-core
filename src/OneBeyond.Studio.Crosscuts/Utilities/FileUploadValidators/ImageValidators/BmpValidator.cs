namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.ImageValidators;

/// <summary>
/// </summary>
public sealed class BmpValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public BmpValidator()
        : base(
              "image/bmp",
              new[] { ".bmp", ".dib" },
              "42-4D")
    {
    }
}
