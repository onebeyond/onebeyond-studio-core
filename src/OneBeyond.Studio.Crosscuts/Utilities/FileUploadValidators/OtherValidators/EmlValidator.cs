using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OtherValidators;

/// <summary>
/// </summary>
public sealed class EmlValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public EmlValidator()
        : base(
              "message/rfc822",
              new[] { ".eml" },
              new[]
              {
                      "58-2D",
                      "52-65-74-75-72-6E-2D-50",
                      "46-72-6F-6D"
              })
    {
    }
}
