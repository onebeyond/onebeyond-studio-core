using System.Collections.Generic;
using System.IO;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.ImageValidators;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OfficeValidators;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.OtherValidators;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators.VideoAudioValidators;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

public sealed class FileValidatorBuilder
{
    private readonly Dictionary<string, IFileContentValidator> _validators = new();

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

    public void ValidateFile(string fileName, string contentType, Stream content)
    {
        EnsureArg.IsNotEmptyOrWhiteSpace(fileName, nameof(fileName));
        EnsureArg.IsNotEmptyOrWhiteSpace(contentType, nameof(contentType));
        EnsureArg.IsNotNull(content, nameof(content));

        if (!_validators.TryGetValue(contentType, out var validator))
        {
            throw new FileContentValidatorException($"Unable to find a validator for contet type: {contentType}.");
        }

        validator.ValidateFileContent(fileName, contentType, content);
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

        validator.ValidateFileContent(fileName, contentType, content);
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
