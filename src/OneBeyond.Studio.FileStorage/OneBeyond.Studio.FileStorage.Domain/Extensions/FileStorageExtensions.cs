using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.FileStorage.Domain.Entities;

namespace OneBeyond.Studio.FileStorage.Domain.Extensions;

/// <summary>
/// File Storage extensions
/// </summary>
public static class FileStorageExtensions
{
    /// <summary>
    /// Delete a file, do not raise an exeption in case if the deletion failed.
    /// </summary>
    /// <param name="fileStorage"></param>
    /// <param name="fileId">Id of a file to be deleted</param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> TryDeleteFileAsync(
        this IFileStorage fileStorage,
        FileRecord.ID fileId,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await fileStorage
                .DeleteFileAsync(fileId, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "File {fileId} was not succesfully removed.", fileId);
            return false;
        }
        return true;
    }
}
