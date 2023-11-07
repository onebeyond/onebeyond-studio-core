using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.Streams;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

public abstract class FileSignatureValidator : IFileContentValidator
{
    protected FileSignatureValidator(
        string contentType,
        IReadOnlyCollection<string> extensions,
        IReadOnlyCollection<string> signatures
        )
    {
        EnsureArg.IsNotNullOrEmpty(contentType, nameof(contentType));
        EnsureArg.IsNotNull(extensions, nameof(extensions));
        EnsureArg.IsNotNull(signatures, nameof(signatures));

        ContentType = contentType;
        Extensions = extensions;
        Signatures = signatures;
    }

    protected FileSignatureValidator(
        string contentType,
        IReadOnlyCollection<string> extensions,
        string signature
        )
        : this(contentType, extensions, new List<string> { signature })
    {
        EnsureArg.IsNotNull(signature, nameof(signature));
    }

    /// <summary>
    /// The mime type this validator validates
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// File extensions this validator (mime type) is expecting
    /// </summary>
    public IReadOnlyCollection<string> Extensions { get; }

    /// <summary>
    /// Expected signature for this file type
    /// </summary>
    public IReadOnlyCollection<string> Signatures { get; }

    /// <summary>
    /// Validate file content
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="contentType">File content type</param>
    /// <param name="content">File stream</param>
    /// <returns></returns>
    public virtual void ValidateFileContent(string fileName, string contentType, Stream content)
    {
        EnsureArg.IsNotNullOrEmpty(fileName, nameof(fileName));
        EnsureArg.IsNotNullOrEmpty(contentType, nameof(contentType));
        EnsureArg.IsNotNull(content, nameof(content));
        EnsureArg.IsGt(content.Length, 0, $"{nameof(content)}.Length");

        ValidateFileContent(
            fileName,
            contentType,
            content.ToByteArray()); // Not an optimal way. It is better to read just requested amount of bytes
    }

    /// <summary>
    /// Validate file content
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="contentType">File content type</param>
    /// <param name="content">File stream</param>
    /// <returns></returns>
    public virtual void ValidateFileContent(string fileName, string contentType, byte[] content)
    {
        EnsureArg.IsNotNullOrEmpty(fileName, nameof(fileName));
        EnsureArg.IsNotNullOrEmpty(contentType, nameof(contentType));
        EnsureArg.IsNotNull(content, nameof(content));
        EnsureArg.IsGt(content.Length, 0, $"{nameof(content)}.Length");

        ValidateFileContent(
            fileName,
            contentType,
            (sizeToRead) => new ReadOnlySpan<byte>(content, 0, sizeToRead > content.Length 
                ? content.Length
                : sizeToRead
                ).ToArray());
    }

    /// <summary>
    /// Validate file content type
    /// </summary>
    /// <param name="fileContentType"></param>
    protected virtual void ValidateFileContentType(string fileContentType)
    {
        if (!string.Equals(ContentType, fileContentType, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new FileContentValidatorException($"File content type {fileContentType} does not correspond the expected content type {ContentType}.");
        }
    }

    /// <summary>
    /// Validate file extension
    /// </summary>
    /// <param name="fileName"></param>
    protected virtual void ValidateFileExtension(string fileName)
    {
        var fileExt = Path.GetExtension(fileName).ToLower();

        if (!Extensions.Any(ext => string.Equals(ext, fileExt, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new FileContentValidatorException($"File extension {fileExt} does not correspond the expected content type {ContentType}. Expected extensions: {string.Join(',', Extensions)}.");
        }
    }

    /// <summary>
    /// Validate file signature
    /// </summary>
    /// <param name="readFromStart"></param>
    protected virtual void ValidateFileSignature(Func<int, byte[]> readFromStart)
    {
        var fileSignatureAsHex = GetFileSignatureAsHex(readFromStart);

        if (!Signatures.Any(fileSignatureAsHex.StartsWith))
        {
            throw new FileContentValidatorException($"Invalid file content.");
        }
    }

    protected string GetFileSignatureAsHex(Func<int, byte[]> readFromStart)
    {
        var signatureLength = Signatures.Max(s => s.Replace("-", "").Length / 2);

        return BitConverter.ToString(readFromStart(signatureLength));
    }

    private void ValidateFileContent(string fileName, string contentType, Func<int, byte[]> readFromStart)
    {
        ValidateFileExtension(fileName);
        ValidateFileContentType(contentType);
        ValidateFileSignature(readFromStart);
    }
}
