namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class PowerpointMacroValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public PowerpointMacroValidator()
        : base(
            "application/vnd.ms-powerpoint.presentation.macroEnabled.12",
            new[] { ".pptm" },
            "50-4B-03-04-14-00-06-00")
    {
    }
}
