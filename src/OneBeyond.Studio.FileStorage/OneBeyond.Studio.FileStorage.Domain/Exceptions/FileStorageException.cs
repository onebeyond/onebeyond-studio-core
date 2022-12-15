using System;
using System.Runtime.Serialization;

namespace OneBeyond.Studio.FileStorage.Domain.Exceptions;

/// <summary>
/// Thrown for all file issues not relating to <see cref="FileNotFoundException"/> or <see cref="FileNotAllowedException"/>.
/// </summary>
[Serializable]
public class FileStorageException : Exception
{
    public FileStorageException(string message)
        : base(message)
    {
    }

    public FileStorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected FileStorageException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
