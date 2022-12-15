using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;

/// <summary>
/// </summary>
public sealed class VisioValidator : FileSignatureValidator
{
    /// <summary>
    /// </summary>
    public VisioValidator()
        : base(
            "application/vnd.ms-visio.viewer",
            new[] { ".vsd", ".vsdx" },
            new[]
            {
                    "50-4B-03-04",
                    "D0-CF-11-E0-A1-B1-1A-E1"
            })
    {
    }
}
