using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.FileStorage.Domain.Options;
using OneBeyond.Studio.FileStorage.Domain.Validations;

namespace OneBeyond.Studio.FileStorage.Domain.Tests;

[TestClass]
public sealed class MimeTypeValidationStrategyTests
{
    [TestMethod]
    public void TestWhitelistStrategyFailsWhenListIsEmpty()
    {
        var options = new MimeTypeValidationOptions
        {
            ValidationMode = MimeTypeValidationMode.Whitelist,
            MimeTypeSignatures = Array.Empty<MimeTypeSignatureOptions>()
        };

        var strategy = new WhitelistMimeTypeValidationStrategy(options);

        var fileContentType = "image/jpeg";

        var fileContentBytes = new byte[] { 0x45, 0x46, 0x47 };

        var fileContentStream = new MemoryStream(fileContentBytes);

        var validationResult = strategy.IsFileAllowed(fileContentBytes, fileContentType);

        Assert.IsFalse(validationResult);

        validationResult = strategy.IsFileAllowed(fileContentStream, fileContentType);

        Assert.IsFalse(validationResult);
    }

    [TestMethod]
    public void TestBlacklistStrategySucceedsWhenListIsEmpty()
    {
        var options = new MimeTypeValidationOptions
        {
            ValidationMode = MimeTypeValidationMode.Blacklist,
            MimeTypeSignatures = Array.Empty<MimeTypeSignatureOptions>()
        };

        var strategy = new BlacklistMimeTypeValidationStrategy(options);

        var fileContentType = "image/jpeg";

        var fileContentBytes = new byte[] { 0x45, 0x46, 0x47 };

        var fileContentStream = new MemoryStream(fileContentBytes);

        var validationResult = strategy.IsFileAllowed(fileContentBytes, fileContentType);

        Assert.IsTrue(validationResult);

        validationResult = strategy.IsFileAllowed(fileContentStream, fileContentType);

        Assert.IsTrue(validationResult);
    }

    [TestMethod]
    public void TestWhitelistStrategyFailsWhenContentTypeIsNotListed()
    {
        var options = new MimeTypeValidationOptions
        {
            ValidationMode = MimeTypeValidationMode.Whitelist,
            MimeTypeSignatures = new[]
            {
                    new MimeTypeSignatureOptions
                    {
                        MimeType = "image/png",
                        Signatures = Array.Empty<string>()
                    }
                }
        };

        var strategy = new WhitelistMimeTypeValidationStrategy(options);

        var fileContentType = "image/jpeg";

        var fileContentBytes = new byte[] { 0x45, 0x46, 0x47 };

        var fileContentStream = new MemoryStream(fileContentBytes);

        var validationResult = strategy.IsFileAllowed(fileContentBytes, fileContentType);

        Assert.IsFalse(validationResult);

        validationResult = strategy.IsFileAllowed(fileContentStream, fileContentType);

        Assert.IsFalse(validationResult);
    }

    [TestMethod]
    public void TestBlacklistStrategySucceedsWhenContentTypeIsNotListed()
    {
        var options = new MimeTypeValidationOptions
        {
            ValidationMode = MimeTypeValidationMode.Blacklist,
            MimeTypeSignatures = new[]
            {
                    new MimeTypeSignatureOptions
                    {
                        MimeType = "image/png",
                        Signatures = Array.Empty<string>()
                    }
                }
        };

        var strategy = new BlacklistMimeTypeValidationStrategy(options);

        var fileContentType = "image/jpeg";

        var fileContentBytes = new byte[] { 0x45, 0x46, 0x47 };

        var fileContentStream = new MemoryStream(fileContentBytes);

        var validationResult = strategy.IsFileAllowed(fileContentBytes, fileContentType);

        Assert.IsTrue(validationResult);

        validationResult = strategy.IsFileAllowed(fileContentStream, fileContentType);

        Assert.IsTrue(validationResult);
    }

    [TestMethod]
    public void TestWhitelistStrategySucceedsWhenContentTypeIsListedWithEmptySignatureList()
    {
        var options = new MimeTypeValidationOptions
        {
            ValidationMode = MimeTypeValidationMode.Whitelist,
            MimeTypeSignatures = new[]
            {
                    new MimeTypeSignatureOptions
                    {
                        MimeType = "image/jpeg",
                        Signatures = Array.Empty<string>()
                    }
                }
        };

        var strategy = new WhitelistMimeTypeValidationStrategy(options);

        var fileContentType = "image/jpeg";

        var fileContentBytes = new byte[] { 0x45, 0x46, 0x47 };

        var fileContentStream = new MemoryStream(fileContentBytes);

        var validationResult = strategy.IsFileAllowed(fileContentBytes, fileContentType);

        Assert.IsTrue(validationResult);

        validationResult = strategy.IsFileAllowed(fileContentStream, fileContentType);

        Assert.IsTrue(validationResult);
    }

    [TestMethod]
    public void TestBlacklistStrategyFailsWhenContentTypeIsListedWithEmptySignatureList()
    {
        var options = new MimeTypeValidationOptions
        {
            ValidationMode = MimeTypeValidationMode.Blacklist,
            MimeTypeSignatures = new[]
            {
                    new MimeTypeSignatureOptions
                    {
                        MimeType = "image/jpeg",
                        Signatures = Array.Empty<string>()
                    }
                }
        };

        var strategy = new BlacklistMimeTypeValidationStrategy(options);

        var fileContentType = "image/jpeg";

        var fileContentBytes = new byte[] { 0x45, 0x46, 0x47 };

        var fileContentStream = new MemoryStream(fileContentBytes);

        var validationResult = strategy.IsFileAllowed(fileContentBytes, fileContentType);

        Assert.IsFalse(validationResult);

        validationResult = strategy.IsFileAllowed(fileContentStream, fileContentType);

        Assert.IsFalse(validationResult);
    }

    [TestMethod]
    public void TestWhitelistStrategySucceedsWhenContentTypeIsListedWithEmptyEntryInSignatureList()
    {
        var options = new MimeTypeValidationOptions
        {
            ValidationMode = MimeTypeValidationMode.Whitelist,
            MimeTypeSignatures = new[]
            {
                    new MimeTypeSignatureOptions
                    {
                        MimeType = "image/jpeg",
                        Signatures = new[] { string.Empty, "45464748" }
                    }
                }
        };

        var strategy = new WhitelistMimeTypeValidationStrategy(options);

        var fileContentType = "image/jpeg";

        var fileContentBytes = new byte[] { 0x45, 0x46, 0x47 };

        var fileContentStream = new MemoryStream(fileContentBytes);

        var validationResult = strategy.IsFileAllowed(fileContentBytes, fileContentType);

        Assert.IsTrue(validationResult);

        validationResult = strategy.IsFileAllowed(fileContentStream, fileContentType);

        Assert.IsTrue(validationResult);
    }

    [TestMethod]
    public void TestBlacklistStrategyFailsWhenContentTypeIsListedWithEmptyEntryInSignatureList()
    {
        var options = new MimeTypeValidationOptions
        {
            ValidationMode = MimeTypeValidationMode.Blacklist,
            MimeTypeSignatures = new[]
            {
                    new MimeTypeSignatureOptions
                    {
                        MimeType = "image/jpeg",
                        Signatures = new[] { string.Empty, "45464748" }
                    }
                }
        };

        var strategy = new BlacklistMimeTypeValidationStrategy(options);

        var fileContentType = "image/jpeg";

        var fileContentBytes = new byte[] { 0x45, 0x46, 0x47 };

        var fileContentStream = new MemoryStream(fileContentBytes);

        var validationResult = strategy.IsFileAllowed(fileContentBytes, fileContentType);

        Assert.IsFalse(validationResult);

        validationResult = strategy.IsFileAllowed(fileContentStream, fileContentType);

        Assert.IsFalse(validationResult);
    }

    [TestMethod]
    public void TestWhitelistStrategySucceedsWhenContentTypeIsListedWithEntryInSignatureList()
    {
        var options = new MimeTypeValidationOptions
        {
            ValidationMode = MimeTypeValidationMode.Whitelist,
            MimeTypeSignatures = new[]
            {
                    new MimeTypeSignatureOptions
                    {
                        MimeType = "image/jpeg",
                        Signatures = new[] { "45464748", "AABBcc" }
                    }
                }
        };

        var strategy = new WhitelistMimeTypeValidationStrategy(options);

        var fileContentType = "image/jpeg";

        var fileContentBytes = new byte[] { 0xAA, 0xBB, 0xCC };

        var fileContentStream = new MemoryStream(fileContentBytes);

        var validationResult = strategy.IsFileAllowed(fileContentBytes, fileContentType);

        Assert.IsTrue(validationResult);

        validationResult = strategy.IsFileAllowed(fileContentStream, fileContentType);

        Assert.IsTrue(validationResult);
    }

    [TestMethod]
    public void TestBlacklistStrategyFailsWhenContentTypeIsListedWithEntryInSignatureList()
    {
        var options = new MimeTypeValidationOptions
        {
            ValidationMode = MimeTypeValidationMode.Blacklist,
            MimeTypeSignatures = new[]
            {
                    new MimeTypeSignatureOptions
                    {
                        MimeType = "image/jpeg",
                        Signatures = new[] { "45464748" }
                    },
                    new MimeTypeSignatureOptions
                    {
                        MimeType = "image/jpeg",
                        Signatures = new[] { "AaBBcc" }
                    }
                }
        };

        var strategy = new BlacklistMimeTypeValidationStrategy(options);

        var fileContentType = "image/jpeg";

        var fileContentBytes = new byte[] { 0xAA, 0xBB, 0xCC };

        var fileContentStream = new MemoryStream(fileContentBytes);

        var validationResult = strategy.IsFileAllowed(fileContentBytes, fileContentType);

        Assert.IsFalse(validationResult);

        validationResult = strategy.IsFileAllowed(fileContentStream, fileContentType);

        Assert.IsFalse(validationResult);
    }
}
