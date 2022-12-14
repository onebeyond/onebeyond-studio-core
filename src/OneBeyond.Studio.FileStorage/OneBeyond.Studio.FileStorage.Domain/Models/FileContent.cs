using System.IO;
using EnsureThat;

namespace OneBeyond.Studio.FileStorage.Domain.Models;

public sealed record FileContent
{
    public FileContent(string name, string contentType, Stream content)
    {
        EnsureArg.IsNotNull(content, nameof(name));
        EnsureArg.IsNotNullOrEmpty(name, nameof(name));
        EnsureArg.IsNotNullOrEmpty(contentType, nameof(contentType));

        Name = name;
        ContentType = contentType;
        Content = content;
    }

    public string Name { get; }
    public string ContentType { get; }
    public Stream Content { get; }
}
