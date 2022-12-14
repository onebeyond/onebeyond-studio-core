using System.Collections.Generic;

#nullable enable

namespace OneBeyond.Studio.FileStorage.Domain.Options;

/// <summary>
/// </summary>
public sealed class MimeTypeValidationOptions
{
    /// <summary>
    /// </summary>
    public MimeTypeValidationMode ValidationMode { get; set; }

    /// <summary>
    /// </summary>
    public IReadOnlyCollection<MimeTypeSignatureOptions> MimeTypeSignatures { get; set; } = null!;
}
