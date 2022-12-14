using System.Collections.Generic;

namespace OneBeyond.Studio.FileStorage.Domain.Options;

/// <summary>
/// </summary>
public sealed class MimeTypeSignatureOptions
{
    /// <summary>
    /// </summary>
    public string MimeType { get; set; }

    /// <summary>
    /// </summary>
    public IReadOnlyCollection<string> Signatures { get; set; }
}
