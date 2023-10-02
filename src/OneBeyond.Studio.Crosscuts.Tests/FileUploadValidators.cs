using System;
using System.IO;
using FluentAssertions;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.ImageValidators;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OtherValidators;
using Xunit;

namespace OneBeyond.Studio.Crosscuts.Tests;

public sealed class FileUploadValidatorsTest
{
    [Fact]
    public void TestFileValidatorShouldAcceptDocuments()
    {
        var validator = new FileValidatorBuilder().AllowPdf();

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/catalunia.pdf"))
            {
                validator.ValidateFile(
                    "catalunia.pdf",
                    "application/pdf",
                    stream);
            }
        };

        action.Should().NotThrow<FileContentValidatorException>();
    }

    [Fact]
    public void TestFileValidatorShouldAcceptImages()
    {
        var validator = new FileValidatorBuilder().AllowImages();

        var jpgAction = () =>
        {
            using (var stream = GetFile("TestFiles/image.jpg"))
            {
                validator.ValidateFile(
                    "image.jpg",
                    "image/jpeg",
                    stream);
            }
        };

        jpgAction.Should().NotThrow<FileContentValidatorException>();

        var bmpAction = () =>
        {
            using (var stream = GetFile("TestFiles/image.bmp"))
            {
                validator.ValidateFile(
                    "image.bmp",
                    "image/bmp",
                    stream);
            }
        };

        bmpAction.Should().NotThrow<FileContentValidatorException>();

        var gifAction = () =>
        {
            using (var stream = GetFile("TestFiles/image.gif"))
            {
                validator.ValidateFile(
                    "image.gif",
                    "image/gif",
                    stream);
            }
        };

        gifAction.Should().NotThrow<FileContentValidatorException>();
    }

    [Fact]
    public void TestFileValidatorShouldNotAcceptDocuments()
    {
        var validator = new FileValidatorBuilder().AllowPng();

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/catalunia.pdf"))
            {
                validator.ValidateFile(
                    "catalunia.pdf",
                    "application/pdf",
                    stream);
            }
        };

        action.Should().Throw<FileContentValidatorException>();
    }

    [Fact]
    public void TestFileValidatorShouldNotAcceptImages()
    {
        var validator = new FileValidatorBuilder().AllowDocuments();

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/image.jpg"))
            {
                validator.ValidateFile(
                    "image.jpg",
                    "image/jpeg",
                    stream);
            }
        };

        action.Should().Throw<FileContentValidatorException>();
    }

    [Fact]
    public void TestFileValidatorShouldAcceptSpreadhsheets()
    {
        var validator = new FileValidatorBuilder().AllowSpreadsheet();

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/spreadsheet.xls"))
            {
                validator.ValidateFile(
                    "spreadsheet.xls",
                    "application/msexcel",
                    stream);
            }
        };

        action.Should().NotThrow<FileContentValidatorException>();

        action = () =>
        {
            using (var stream = GetFile("TestFiles/spreadsheet.xlsx"))
            {
                validator.ValidateFile(
                    "spreadsheet.xlsx",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.document",
                    stream);
            }
        };

        action.Should().NotThrow<FileContentValidatorException>();

        action = () =>
        {
            using (var stream = GetFile("TestFiles/spreadsheet.xlsm"))
            {
                validator.ValidateFile(
                    "spreadsheet.xlsm",
                    "application/vnd.ms-excel.sheet.macroEnabled.12",
                    stream);
            }
        };

        action.Should().NotThrow<FileContentValidatorException>();
    }

    [Fact]
    public void TestFileValidatorShouldNotAcceptSpreadsheets()
    {
        var validator = new FileValidatorBuilder().AllowPng();

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/spreadsheet.xls"))
            {
                validator.ValidateFile(
                    "spreadsheet.xls",
                    "application/msexcel",
                    stream);
            }
        };

        action.Should().Throw<FileContentValidatorException>();

    }

    [Fact]
    public void TestMultipleSignatureFile()
    {
        var jpgValidator = new JpgValidator();

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/image.jpg"))
            {
                jpgValidator.ValidateFileContent("image.jpg", "image/jpeg", stream);
            }
        };

        action.Should().NotThrow<FileContentValidatorException>();
    }

    [Fact]
    public void TestSingleSignatureFile()
    {
        var pdfValidator = new PdfValidator();

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/catalunia.pdf"))
            {
                pdfValidator.ValidateFileContent("catalunia.pdf", "application/pdf", stream);
            }
        };

        action.Should().NotThrow<FileContentValidatorException>();
    }

    [Fact]
    public void TestFileValidatorShouldAcceptText()
    {
        var validator = new FileValidatorBuilder().AllowSimpleText();

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/Text.txt"))
            {
                validator.ValidateFile(
                    "Text.txt",
                    "text/plain",
                    stream);
            }
        };

        action.Should().NotThrow<FileContentValidatorException>();
    }

    [Fact]
    public void TestFileExtensionDoesNotCorrepsondContentType()
    {
        var validator = new JpgValidator();

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/image.jpg"))
            {
                validator.ValidateFileContent(
                    "image.html", // <--- Wrong extension!
                    "image/jpeg",
                    stream);
            }
        };

        action.Should().Throw<FileContentValidatorException>();
    }

    [Fact]
    public void TestFileValidatorShouldAcceptImageOfLessThan400KB()
    {
        var validator = new FileValidatorBuilder()
            .AllowImages()
            .HasMaxSizeInKB(400);

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/image.gif"))
            {
                validator.ValidateFile(
                    "image.gif",
                    "image/gif",
                    stream);
            }
        };

        action.Should().NotThrow<FileSizeValidatorException>();
    }

    [Fact]
    public void TestFileValidatorShouldNotAcceptImageOfMoreThan300KB()
    {
        var validator = new FileValidatorBuilder()
            .AllowImages()
            .HasMaxSizeInKB(300);

        var action = () =>
        {
            using (var stream = GetFile("TestFiles/image.gif"))
            {
                validator.ValidateFile(
                    "image.gif",
                    "image/gif",
                    stream);
            }
        };

        action.Should().Throw<FileSizeValidatorException>();
    }

    private static Stream GetFile(string filePath)
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        var file = new FileInfo(Path.Combine(dir, filePath));
        file.Exists.Should().BeTrue();
        return file.OpenRead();
    }
}
