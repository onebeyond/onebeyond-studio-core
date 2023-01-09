namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.ImageValidators;

/// <summary>
/// </summary>
public sealed class PngValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public PngValidator()
        : base(
              "image/png",
              new[] { ".png" },
              "89-50-4E-47-0D-0A-1A-0A")
    {
    }
}
