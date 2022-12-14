using System.IO;
using OneBeyond.Studio.FileStorage.Domain.Options;

#nullable enable

namespace OneBeyond.Studio.FileStorage.Domain.Validations;

internal sealed class WhitelistMimeTypeValidationStrategy : MimeTypeValidationStrategy
{
    public WhitelistMimeTypeValidationStrategy(MimeTypeValidationOptions mimeTypeValidationOptions)
        : base(mimeTypeValidationOptions)
    {
    }

    public override bool IsFileAllowed(Stream content, string mimeType)
        => IsFileCoveredByOptions(content, mimeType);

    public override bool IsFileAllowed(byte[] content, string mimeType)
        => IsFileCoveredByOptions(content, mimeType);
}
