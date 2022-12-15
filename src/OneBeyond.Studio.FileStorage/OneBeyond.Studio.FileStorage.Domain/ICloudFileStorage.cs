using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.FileStorage.Domain;

/// <summary>
/// Interface for interacting directly with cloud storage asynchronously. This requests 
/// URLs with shared access tokens for upload and download, no additional checks or 
/// verifications are performed. This is currently up to the user.
/// </summary>
public interface ICloudFileStorage
{
    /// <summary>
    /// Request the Download URL from a cloud storage blob, given a file ID.
    /// </summary>    
    Task<Uri> GetDownloadUrlAsync(string fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Request the Upload URL from a cloud storage blob, given a file ID.
    /// </summary>    
    Task<Uri> GetUploadUrlAsync(string fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Request the Delete URL from a cloud storage blob, given a file ID.
    /// </summary>
    Task<Uri> GetDeleteUrlAsync(string fileId, CancellationToken cancellationToken);
}
