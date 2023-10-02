using System.Collections.Generic;
using System.IO;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.Streams;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.ImageValidators;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OtherValidators;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.VideoAudioValidators;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

public sealed class FileValidatorBuilder
{
    private readonly Dictionary<string, IFileContentValidator> _validators = new();
    private int? _maxFileSize = null;

    public FileValidatorBuilder AddValidator(IFileContentValidator validator)
    {
        EnsureArg.IsNotNull(validator, nameof(validator));

        if (_validators.ContainsKey(validator.ContentType))
        {
            _validators[validator.ContentType] = validator; //replace existing validator with another one
        }
        else
        {
            _validators.Add(validator.ContentType, validator);
        }

        return this;
    }

    public FileValidatorBuilder AllowEmail()
        => AddValidator(new EmlValidator());

    public FileValidatorBuilder AllowPdf()
        => AddValidator(new PdfValidator());

    public FileValidatorBuilder AllowPng()
        => AddValidator(new PngValidator());

    public FileValidatorBuilder AllowJPeg()
        => AddValidator(new JpgValidator());

    public FileValidatorBuilder AllowBmp()
        => AddValidator(new BmpValidator());

    public FileValidatorBuilder AllowGif()
        => AddValidator(new GifValidator());

    public FileValidatorBuilder AllowWord()
        => AddValidator(new WordValidator())
            .AddValidator(new WordXValidator())
            .AddValidator(new WordMacroValidator());

    public FileValidatorBuilder AllowSpreadsheet()
        => AddValidator(new SpreadsheetValidator())
            .AddValidator(new SpreadsheetXlsValidator())
            .AddValidator(new SpreadsheetMacroValidator())
            .AddValidator(new SpreadsheetBinaryValidator())
            .AddValidator(new SpreadsheetXValidator())
            .AddValidator(new SpreadsheetXSheetValidator());

    public FileValidatorBuilder AllowPowerpoint()
        => AddValidator(new PowerpointValidator())
            .AddValidator(new PowerpointMacroValidator())
            .AddValidator(new PowerpointXValidator());

    public FileValidatorBuilder AllowVideo()
        => AddValidator(new SpreadsheetValidator())
            .AddValidator(new Mp4Validator())
            .AddValidator(new WmvValidator())
            .AddValidator(new QuicktimeValidator())
            .AddValidator(new MpegValidator());

    public FileValidatorBuilder AllowAudio()
        => AddValidator(new WavValidator());

    public FileValidatorBuilder AllowVisio()
        => AddValidator(new VisioValidator());

    public FileValidatorBuilder AllowSimpleText()
        => AddValidator(new TxtValidator());

    public FileValidatorBuilder HasMaxSize(int maxFileSize)
    {
        _maxFileSize = maxFileSize;
        return this;
    }

    public void ValidateFile(string fileName, string contentType, Stream content)
    {
        EnsureArg.IsNotEmptyOrWhiteSpace(fileName, nameof(fileName));
        EnsureArg.IsNotEmptyOrWhiteSpace(contentType, nameof(contentType));
        EnsureArg.IsNotNull(content, nameof(content));

        if (!_validators.TryGetValue(contentType, out var validator))
        {
            throw new FileContentValidatorException($"Unable to find a validator for contet type: {contentType}.");
        }

        var contentInByte = content.ToByteArray();
        ValidateFileSize(contentInByte);
        validator.ValidateFileContent(fileName, contentType, contentInByte);
    }

    public void ValidateFile(string fileName, string contentType, byte[] content)
    {
        EnsureArg.IsNotEmptyOrWhiteSpace(fileName, nameof(fileName));
        EnsureArg.IsNotEmptyOrWhiteSpace(contentType, nameof(contentType));
        EnsureArg.IsNotNull(content, nameof(content));

        if (!_validators.TryGetValue(contentType, out var validator))
        {
            throw new FileContentValidatorException($"Unable to find a validator for contet type: {contentType}.");
        }

        ValidateFileSize(content);
        validator.ValidateFileContent(fileName, contentType, content);
    }

    public void ValidateFileSize(byte[] content)
    {
        if (_maxFileSize.HasValue && content.Length > _maxFileSize.Value)
        {
            throw new FileSizeValidatorException($"The file exceeds the maximum allowed size ({_maxFileSize / 1_000_000} Mb)");
        }
    }

    public FileValidatorBuilder AllowImages()
        => AllowJPeg()
        .AllowPng()
        .AllowBmp()
        .AllowGif();

    public FileValidatorBuilder AllowDocuments()
        => AllowPdf()
        .AllowWord()
        .AllowSpreadsheet()
        .AllowPowerpoint();
}
