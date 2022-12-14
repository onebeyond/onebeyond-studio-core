using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnsureThat;
using OneBeyond.Studio.FileStorage.Domain.Options;

#nullable enable

namespace OneBeyond.Studio.FileStorage.Domain.Validations;

internal abstract class MimeTypeValidationStrategy
{
    protected MimeTypeValidationStrategy(MimeTypeValidationOptions mimeTypeValidationOptions)
    {
        EnsureArg.IsNotNull(mimeTypeValidationOptions, nameof(mimeTypeValidationOptions));
        EnsureArg.IsNotNull(mimeTypeValidationOptions.MimeTypeSignatures, nameof(mimeTypeValidationOptions.MimeTypeSignatures));
        EnsureArg.IsTrue(
            mimeTypeValidationOptions.MimeTypeSignatures
                .SelectMany((mimeTypeOptions) => mimeTypeOptions.Signatures.Select((signature) => signature.Length))
                .All((signatureLength) => signatureLength % 2 == 0) == true,
            nameof(mimeTypeValidationOptions.MimeTypeSignatures),
            (options) => options.WithMessage("Mime type signature length is not multiple of 2"));

        MimeTypeValidationOptions = mimeTypeValidationOptions;
    }

    protected MimeTypeValidationOptions MimeTypeValidationOptions { get; }

    public abstract bool IsFileAllowed(Stream content, string mimeType);

    public abstract bool IsFileAllowed(byte[] content, string mimeType);

    protected bool IsFileCoveredByOptions(Stream content, string mimeType)
    {
        return IsFileCoveredByOptions(
            (signatureFromContentBytes, signatureFromContentLength) =>
            {
                signatureFromContentLength = content.Read(
                    signatureFromContentBytes,
                    0,
                    signatureFromContentLength);
                content.Seek(0, SeekOrigin.Begin);
                return signatureFromContentLength;
            },
            mimeType);
    }

    protected bool IsFileCoveredByOptions(byte[] content, string mimeType)
    {
        return IsFileCoveredByOptions(
            (signatureFromContentBytes, signatureFromContentLength) =>
            {
                signatureFromContentLength = Math.Min(signatureFromContentLength, content.Length);
                Array.Copy(content, 0, signatureFromContentBytes, 0, signatureFromContentLength);
                return signatureFromContentLength;
            },
            mimeType);
    }

    private bool IsFileCoveredByOptions(Func<byte[], int, int> getContentSignature, string mimeType)
    {
        var mimeTypeSignatures = ListSignaturesByMimeType(mimeType);

        if (!mimeTypeSignatures.Any())
        {
            return false;
        }

        var mimeTypeSignatureValues = mimeTypeSignatures
            .SelectMany((mimeTypeSignature) => mimeTypeSignature)
            .ToArray();

        if (mimeTypeSignatureValues.Length == 0
            || mimeTypeSignatureValues.Any((signature) => signature.Length == 0))
        {
            return true;
        }

        var signatureMaxLength = mimeTypeSignatureValues.Max((signature) => signature.Length) / 2;

        var signatureFromContentBytes = new byte[signatureMaxLength];
        var signatureFromContentLength = getContentSignature(
            signatureFromContentBytes,
            signatureFromContentBytes.Length);
        var signatureFromContent = BitConverter.ToString(
                signatureFromContentBytes,
                0,
                signatureFromContentLength)
            .Replace("-", string.Empty);

        return mimeTypeSignatureValues
            .Any((signature) =>
                signature.Length <= signatureFromContent.Length
                && signatureFromContent.StartsWith(signature, StringComparison.OrdinalIgnoreCase));
    }

    private IEnumerable<IReadOnlyCollection<string>> ListSignaturesByMimeType(string mimeType)
        => MimeTypeValidationOptions.MimeTypeSignatures
            .Where((mimeTypeSignature) =>
                string.Equals(mimeTypeSignature.MimeType, mimeType, StringComparison.OrdinalIgnoreCase))
            .Select((mimeTypeSignature) =>
                mimeTypeSignature.Signatures);
}
