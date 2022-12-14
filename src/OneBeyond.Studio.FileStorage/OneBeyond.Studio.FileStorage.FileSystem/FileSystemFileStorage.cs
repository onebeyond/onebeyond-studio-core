using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using EnsureThat;
using OneBeyond.Studio.FileStorage.Domain;
using OneBeyond.Studio.FileStorage.Domain.Entities;
using OneBeyond.Studio.FileStorage.Domain.Exceptions;
using OneBeyond.Studio.FileStorage.Domain.Models;
using OneBeyond.Studio.FileStorage.Domain.Options;
using OneBeyond.Studio.FileStorage.FileSystem.Options;

namespace OneBeyond.Studio.FileStorage.FileSystem;

internal sealed class FileSystemFileStorage : FileStorageBase
{
    private readonly string _storageRootPath;
    private readonly bool _allowDownloadUrl;

    public FileSystemFileStorage(
        MimeTypeValidationOptions mimeTypeValidationOptions,
        FileSystemFileStorageOptions fileStorageOptions)
        : base(mimeTypeValidationOptions)
    {
        EnsureArg.IsNotNull(fileStorageOptions, nameof(fileStorageOptions));

        _storageRootPath = Environment.ExpandEnvironmentVariables(fileStorageOptions.StorageRootPath);
        _allowDownloadUrl = fileStorageOptions.AllowDownloadUrl;
    }


    protected override Task DoDeleteFileContentAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
    {
        File.Delete(GetFilePath(fileId));
        return Task.CompletedTask;
    }

    protected override Task<Stream> DoDownloadFileContentAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<Stream>(File.OpenRead(GetFilePath(fileId)));
    }

    protected override Task<FileContent> DoDownloadFileAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new FileContent(
            fileId.ToString(),
            "application/octet-stream",
            File.OpenRead(GetFilePath(fileId))
            ));
    }

    protected override async Task DoUploadFileContentAsync(
        FileRecord fileRecord,
        Stream fileContent,
        CancellationToken cancellationToken)
    {
        using (var fileStream = File.OpenWrite(GetFilePath(fileRecord.Id)))
        {
            fileContent.Seek(0, SeekOrigin.Begin);
            await fileContent.CopyToAsync(fileStream).ConfigureAwait(false);
        }
    }

    protected override Task DoCopyFileAsync(
        FileRecord fromFileRecord,
        FileRecord toFileRecord,
        CancellationToken cancellationToken)
    {
        return fromFileRecord.Id == toFileRecord.Id
            ? throw new FileStorageException("You cannot copy a file entry to itself")
            : !File.Exists(GetFilePath(fromFileRecord.Id))
            ? throw new Domain.Exceptions.FileNotFoundException(fromFileRecord.Id)
            : CopyFileAsync(GetFilePath(fromFileRecord.Id), GetFilePath(toFileRecord.Id), cancellationToken);
    }

    protected override Task<Uri> DoGetFileUrlAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken)
    {
        return _allowDownloadUrl
            ? Task.FromResult(new Uri($"file:///{HttpUtility.UrlPathEncode(GetFilePath(fileId))}"))
            : throw new NotSupportedException("Cannot request a file url in file system storage for security reasons.");
    }



    private string GetFilePath(FileRecord.ID fileId)
        => Path.Combine(_storageRootPath, fileId.ToString());

    private static async Task CopyFileAsync(
        string sourceFilePath,
        string destinationFilePath,
        CancellationToken cancellationToken)
    {
        var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
        var bufferSize = 4096;

        using (var sourceStream =
              new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions))
        {
            using (var destinationStream =
                  new FileStream(destinationFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, fileOptions))
            {
                await sourceStream
                    .CopyToAsync(destinationStream, bufferSize, cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
        }
    }
}
