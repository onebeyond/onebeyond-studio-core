using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class PowerpointXValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public PowerpointXValidator()
        : base(
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            new[] { ".pptx" },
            "50-4B-03-04-14-00-06-00")
    {
    }
}
