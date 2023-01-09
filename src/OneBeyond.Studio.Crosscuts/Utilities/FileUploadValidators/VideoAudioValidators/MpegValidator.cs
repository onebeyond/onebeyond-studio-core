namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.VideoAudioValidators;

/// <summary>
/// </summary>
public sealed class MpegValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public MpegValidator()
        : base(
            "video/mpeg",
            new[] { ".mpg", ".mpeg" },
            new[]
            {
                    "00-00-01-00",
                    "00-00-01-BA",
                    "FF-D8",
                    "00-00-01-00",
                    "FF-D8"
            })
    {
    }
}
