namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.ImageValidators;

/// <summary>
/// </summary>
public sealed class GifValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public GifValidator()
        : base(
              "image/gif",
              new[] { ".gif" },
              "47-49-46-38")
    {
    }
}
