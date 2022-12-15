namespace OneBeyond.Studio.FileStorage.Domain.Exceptions;

/// <summary>
/// </summary>
public sealed class FileNotAllowedException : FileStorageException
{
    /// <summary>
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fileContentType"></param>
    public FileNotAllowedException(string fileName, string fileContentType)
        : base($"File '{fileName}' with '{fileContentType}' content type is not allowed for uploading")
    {
    }
}
