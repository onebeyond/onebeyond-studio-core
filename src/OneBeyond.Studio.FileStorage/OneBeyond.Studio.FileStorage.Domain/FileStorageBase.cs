using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using MoreLinq;
using OneBeyond.Studio.FileStorage.Domain.Entities;
using OneBeyond.Studio.FileStorage.Domain.Exceptions;
using OneBeyond.Studio.FileStorage.Domain.Models;
using OneBeyond.Studio.FileStorage.Domain.Options;
using OneBeyond.Studio.FileStorage.Domain.Validations;

#nullable enable

namespace OneBeyond.Studio.FileStorage.Domain;

public abstract class FileStorageBase : IFileStorage
{
    private readonly MimeTypeValidationStrategy _mimeTypeValidationStrategy;

    protected FileStorageBase(MimeTypeValidationOptions mimeTypeValidationOptions)
    {
        EnsureArg.IsNotNull(mimeTypeValidationOptions, nameof(mimeTypeValidationOptions));

        _mimeTypeValidationStrategy = mimeTypeValidationOptions.ValidationMode switch
        {
            MimeTypeValidationMode.Blacklist
                => new BlacklistMimeTypeValidationStrategy(mimeTypeValidationOptions),
            MimeTypeValidationMode.Whitelist
                => new WhitelistMimeTypeValidationStrategy(mimeTypeValidationOptions),
            _
                => throw new ArgumentOutOfRangeException(
                    $"Unexpected validation mode {mimeTypeValidationOptions.ValidationMode}")
        };
    }

    public virtual async Task<FileRecord> UploadFileAsync(
        string fileName,
        Stream fileContent,
        string fileContentType,
        CancellationToken cancellationToken)
    {
        if (!_mimeTypeValidationStrategy.IsFileAllowed(fileContent, fileContentType))
        {
            throw new FileNotAllowedException(fileName, fileContentType);
        }

        var fileRecord = new FileRecord(fileName, fileContent.Length, fileContentType);

        await DoUploadFileContentAsync(fileRecord, fileContent, cancellationToken).ConfigureAwait(false);

        return fileRecord;
    }

    public async Task<FileRecord> UploadFileAsync(
        string fileName,
        byte[] fileContent,
        string fileContentType,
        CancellationToken cancellationToken)
    {
        if (!_mimeTypeValidationStrategy.IsFileAllowed(fileContent, fileContentType))
        {
            throw new FileNotAllowedException(fileName, fileContentType);
        }

        using (var fileContentStream = new MemoryStream(fileContent))
        {
            return await UploadFileAsync(
                fileName,
                fileContentStream,
                fileContentType,
                cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task UpdateFileContentAsync(
        FileRecord fileRecord,
        Stream fileContent,
        string fileContentType,
        CancellationToken cancellationToken)
    {
        if (!_mimeTypeValidationStrategy.IsFileAllowed(fileContent, fileContentType))
        {
            throw new FileNotAllowedException(fileRecord.Name, fileContentType);
        }

        fileRecord.UpdateContentInfo(
            fileContent.Length,
            string.IsNullOrWhiteSpace(fileContentType)
                ? fileRecord.ContentType
                : fileContentType);

        await DoUploadFileContentAsync(fileRecord, fileContent, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateFileContentAsync(
        FileRecord fileRecord,
        byte[] fileContent,
        string fileContentType,
        CancellationToken cancellationToken)
    {
        if (!_mimeTypeValidationStrategy.IsFileAllowed(fileContent, fileContentType))
        {
            throw new FileNotAllowedException(fileRecord.Name, fileContentType);
        }

        using (var fileContentStream = new MemoryStream(fileContent))
        {
            await UpdateFileContentAsync(
                fileRecord,
                fileContentStream,
                fileContentType,
                cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Copy file
    /// </summary>
    /// <param name="fileRecord">Record for a file to be copied</param>
    /// <param name="fileName">New file name</param>    
    /// <returns>File record for the file copy</returns>
    public async Task<FileRecord> CopyFileAsync(
        FileRecord fileRecord,
        string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        var newFileRecord = fileRecord.Copy(fileName);

        await DoCopyFileAsync(fileRecord, newFileRecord, cancellationToken).ConfigureAwait(false);

        return newFileRecord;
    }

    public Task<Stream> DownloadFileContentAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotDefault(fileId, nameof(fileId));

        return DoDownloadFileContentAsync(fileId, cancellationToken);
    }

    /// <summary>
    /// Download file stream + metadata
    /// </summary>
    public Task<FileContent> DownloadFileAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotDefault(fileId, nameof(fileId));

        return DoDownloadFileAsync(fileId, cancellationToken);
    }

    public async Task<Stream> DownloadFileContentsAsZipAsync(
        IEnumerable<FileRecord> fileRecords,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(fileRecords, nameof(fileRecords));

        var uniqueFileNameGenerator = new UniqueFileNameGenerator();

        // Download all file contents in parallel.
        var fileContentTasks = fileRecords
            .Select((fileRecord) =>
            {
                var fileName = uniqueFileNameGenerator.GetFileName(fileRecord.Name);

                var fileContentTask = DoDownloadFileContentAsync(fileRecord.Id, cancellationToken);

                return (fileName, fileContentTask);
            })
            .ToDictionary(
                (fileContentTask) => fileContentTask.fileName,
                (fileContentTask) => fileContentTask.fileContentTask);

        await Task.WhenAll(fileContentTasks.Values).ConfigureAwait(false);

        // Pack all downloaded file contents into a zip.
        var zipStream = new MemoryStream();
        var resultZipStream = zipStream;
        try
        {
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var fileContentTask in fileContentTasks)
                {
                    var zipEntry = zip.CreateEntry(fileContentTask.Key);

                    var zipEntryContent = fileContentTask.Value.Result;

                    using (var zipEntryStream = zipEntry.Open())
                    {
                        await zipEntryContent.CopyToAsync(zipEntryStream, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
            }

            zipStream.Seek(0, SeekOrigin.Begin);

            zipStream = null!;

            return resultZipStream;
        }
        finally
        {
            zipStream?.Dispose();
        }
    }

    public Task DeleteFileAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
        => DoDeleteFileContentAsync(fileId, cancellationToken);

    /// <summary>
    /// Provide url to download a file directly
    /// </summary>
    /// <param name="fileId">Id of the file</param>
    /// <returns>Url to the file with read permissions</returns>
    public Task<Uri> GetFileUrlAsync(FileRecord.ID fileId, CancellationToken cancellationToken)
        => DoGetFileUrlAsync(fileId, cancellationToken);

    /// <summary>
    /// Provide url to download a file directory
    /// </summary>
    protected abstract Task<Uri> DoGetFileUrlAsync(FileRecord.ID fileId, CancellationToken cancellationToken);

    protected abstract Task DoUploadFileContentAsync(
        FileRecord fileRecord,
        Stream fileContent,
        CancellationToken cancellationToken);

    /// <summary>
    /// Copy file
    /// </summary>
    /// <param name="fromFileRecord">Source file record</param>
    /// <param name="toFileRecord">Destination file record</param>
    /// <returns></returns>
    protected abstract Task DoCopyFileAsync(
                FileRecord fromFileRecord,
                FileRecord toFileRecord,
                CancellationToken cancellationToken);

    protected abstract Task<Stream> DoDownloadFileContentAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Download file straem + metadata
    /// </summary>
    protected abstract Task<FileContent> DoDownloadFileAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken);

    protected abstract Task DoDeleteFileContentAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken);

    private sealed class UniqueFileNameGenerator
    {
        private readonly Dictionary<string, int> _fileNames;

        public UniqueFileNameGenerator()
        {
            _fileNames = new Dictionary<string, int>();
        }

        public string GetFileName(string fileName)
        {
            EnsureArg.IsNotNullOrWhiteSpace(fileName, nameof(fileName));

            var upperCasedFileName = fileName.ToUpper();

            if (_fileNames.TryGetValue(upperCasedFileName, out var fileNameIndex))
            {
                ++fileNameIndex;
                _fileNames[upperCasedFileName] = fileNameIndex;

                return $"{Path.GetFileNameWithoutExtension(fileName)} ({fileNameIndex}){Path.GetExtension(fileName)}";
            }
            else
            {
                _fileNames.Add(upperCasedFileName, 0);
                return fileName;
            }
        }
    }
}
