namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.VideoAudioValidators;

/// <summary>
/// </summary>
public sealed class WmvValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public WmvValidator()
        : base(
            "video/x-ms-wmv",
            new[] { ".wmv" },
            "30-26-B2-75-8E-66-CF-11-A6-D9-00-AA-00-62-CE-6C")
    {
    }
}
