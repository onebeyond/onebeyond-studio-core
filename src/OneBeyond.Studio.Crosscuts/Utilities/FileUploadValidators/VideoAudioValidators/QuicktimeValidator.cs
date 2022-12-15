using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.VideoAudioValidators;

/// <summary>
/// </summary>
public sealed class QuicktimeValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public QuicktimeValidator()
        : base(
            "video/quicktime",
            new[] { ".mov", ".qt" },
            new[]
            {
                    "00",
                    "00-00-00-14-66-74-79-70-71-74-20-20",
                    "66-74-79-70-71-74-20-20",
                    "6D-6F-6F-76"
            })
    {
    }
}
