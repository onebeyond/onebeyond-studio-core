using OneBeyond.Studio.FileStorage.Domain.Entities;

namespace OneBeyond.Studio.FileStorage.Domain.Exceptions;

/// <summary>
/// Thrown when file is not found.
/// </summary>
public sealed class FileNotFoundException : FileStorageException
{
    /// <summary>
    /// </summary>
    /// <param name="fileId">Id of the missing file</param>
    public FileNotFoundException(FileRecord.ID fileId)
        : base($"Unable to find file with ID {fileId}")
    {
    }
}
