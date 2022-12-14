using System.IO;
using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities.Dto;

/// <summary>
/// </summary>
public sealed record FileContentDto
{
    /// <summary>
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="contentType"></param>
    public FileContentDto(string name, string contentType, Stream content)
    {
        EnsureArg.IsNotNull(content, nameof(name));
        EnsureArg.IsNotNullOrEmpty(name, nameof(name));
        EnsureArg.IsNotNullOrEmpty(contentType, nameof(contentType));

        Name = name;
        ContentType = contentType;
        Content = content;
    }

    /// <summary>
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// </summary>
    public string ContentType { get; }
    /// <summary>
    /// </summary>
    public Stream Content { get; }
}
