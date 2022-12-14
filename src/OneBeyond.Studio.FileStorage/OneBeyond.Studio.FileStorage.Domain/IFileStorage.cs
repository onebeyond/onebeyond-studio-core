using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.FileStorage.Domain.Entities;
using OneBeyond.Studio.FileStorage.Domain.Models;

namespace OneBeyond.Studio.FileStorage.Domain;

/// <summary>
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Upload a file
    /// </summary>
    /// <param name="fileName">Name of the file to be uploaded</param>
    /// <param name="fileContentType">File content type</param>
    /// <param name="fileContent">File data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FileRecord> UploadFileAsync(
        string fileName,
        Stream fileContent,
        string fileContentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Upload a file
    /// </summary>
    /// <param name="fileName">Name of the file to be uploaded</param>
    /// <param name="fileContentType">File content type</param>
    /// <param name="fileContent">File data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FileRecord> UploadFileAsync(
        string fileName,
        byte[] fileContent,
        string fileContentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Update a file, earlier uploaded, with the new content
    /// </summary>
    /// <param name="fileRecord">Earlier uploaded file record</param>
    /// <param name="fileContentType">New file content type</param>
    /// <param name="fileContent">New file data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateFileContentAsync(
        FileRecord fileRecord,
        Stream fileContent,
        string fileContentType = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a file, earlier uploaded, with the new content
    /// </summary>
    /// <param name="fileRecord">Earlier uploaded file record</param>
    /// <param name="fileContent">New file content type</param>
    /// <param name="fileContentType">New file data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateFileContentAsync(
        FileRecord fileRecord,
        byte[] fileContent,
        string fileContentType = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Copy a file
    /// </summary>
    /// <param name="fileRecord">Record for a file to be copied</param>
    /// <param name="fileName">New file name</param>
    /// <param name="cancellationToken"></param>
    /// <returns>File record for the copy of the file</returns>
    Task<FileRecord> CopyFileAsync(
        FileRecord fileRecord,
        string fileName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Download a file as a stream
    /// </summary>
    /// <param name="fileId">Id of the file to be downloaded</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> DownloadFileContentAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Download file stream + metadata
    /// </summary>
    /// <param name="fileId">Id of the file to be downloaded</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<FileContent> DownloadFileAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Downloads a set of files as a zipped stream.
    /// </summary>
    /// <param name="fileRecords">Lis of Ids of files to be included into a zip stream</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> DownloadFileContentsAsZipAsync(
        IEnumerable<FileRecord> fileRecords,
        CancellationToken cancellationToken);

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="fileId">Id of the file to delete</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteFileAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Provide url to download a file directly.
    /// </summary>
    /// <param name="fileId">Id of the file</param>
    /// <param name="cancellationToken"></param>    
    /// <returns>Url to the file with read permissions</returns>
    Task<Uri> GetFileUrlAsync(
        FileRecord.ID fileId,
        CancellationToken cancellationToken);
}
