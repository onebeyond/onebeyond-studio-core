using System.Linq;
using System.Text.RegularExpressions;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.VideoAudioValidators;

public sealed class Mp4Validator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public Mp4Validator()
        : base(
            "video/mp4",
            new[] { ".mp4" },
            new[]
            {
                    "66-74-79-70-33-67-70-35",
                    "66-74-79-70-4D-53-4E-56",
                    "66-74-79-70-69-73-6F-6D",
                    "66-74-79-70-6D-70-34-32"
            })
    {
    }

    protected override void ValidateFileSignature(System.Func<int, byte[]> readFromStart)
    {
        var fileSignatureAsHex = GetFileSignatureAsHex(readFromStart);

        if (!(Signatures.Any(fileSignatureAsHex.StartsWith)
                || Regex.IsMatch(fileSignatureAsHex, @"(?:[\dA-F]{2}-){4}(66-74-79-70)(?:[-\dA-F]{2})*", RegexOptions.None, Regex.InfiniteMatchTimeout)))
        {
            throw new FileContentValidatorException($"Invalid file content.");
        }
    }
}
