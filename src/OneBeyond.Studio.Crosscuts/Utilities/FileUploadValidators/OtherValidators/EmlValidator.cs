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
                "46-72-6F-6D",
                "52-65-74-75-72-6E-2D",
                "78-2D-73-74-6F-72-65-2D-69-6E-66-6F-3A",
                "44-61-74-65-3A-20",
                "41-52-43-2D-53-65-61-6C-3A",
                "54-6F-3A-20",
                "52-65-63-65-69-76-65-64-3A-20",
                "43-6F-6E-74-65-6E-74-2D-54-79-70-65-3A-20-6D-75",
                "44-65-6C-69-76-65-72-65-64-2D-54-6F-3A-20",
                "4D-65-73-73-61-67-65-2D-49-44-3A-20-3C",
                "52-65-74-75-72-6E-2D-52-65-63-65-69-70-74-2D-54"
            })
    {
    }
}
