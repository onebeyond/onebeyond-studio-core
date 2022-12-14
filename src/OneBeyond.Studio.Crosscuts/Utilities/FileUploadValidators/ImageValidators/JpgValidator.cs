using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.ImageValidators;

/// <summary>
/// </summary>
public sealed class JpgValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public JpgValidator()
        : base(
              "image/jpeg",
              new[] { ".jpg", ".jpeg", ".jpe", ".jif", ".jfif" },
              new[]
              {
                      "FF-D8-FF-DB",
                      "FF-D8-FF-E0",
                      "FF-D8-FF-E0-00-10-4A-46-49-46-00-01",
                      "FF-D8-FF-E1"
              })
    {
    }
}
