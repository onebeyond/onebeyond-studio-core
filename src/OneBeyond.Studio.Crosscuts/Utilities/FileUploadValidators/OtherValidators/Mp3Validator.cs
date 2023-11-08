namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OtherValidators;

public sealed class Mp3Validator : FileSignatureValidator
{
    public Mp3Validator()
        : base(
            "audio/mpeg",
            new[] { ".mp3" },
            new[]
            {
                "FF-FB",
                "49-44-33",
                "FF-F2"
            })
    {
    }
}