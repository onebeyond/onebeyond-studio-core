namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.VideoAudioValidators;

/// <summary>
/// </summary>
public sealed class WavValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public WavValidator()
        : base(
              "audio/wav",
              new[] { ".wav" },
              "52-49-46-46")
    {
    }
}
