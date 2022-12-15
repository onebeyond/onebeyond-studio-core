using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class PowerpointValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public PowerpointValidator()
        : base(
            "application/vnd.ms-powerpoint",
            new[] { ".ppt" },
            new[]
            {
                    "00-6E-1E-F0",
                    "0F-00-E8-03",
                    "A0-46-1D-F0",
                    "D0-CF-11-E0-A1-B1-1A-E1",
                    "FD-FF-FF-FF-04"
            })
    {
    }
}
