using System;
using System.Runtime.Serialization;
using OneBeyond.Studio.FileStorage.Domain.Exceptions;

namespace OneBeyond.Studio.FileStorage.Azure.Exceptions;

[Serializable]
public sealed class AzureStorageException : FileStorageException
{
    public AzureStorageException(string message) : base(message)
    {
    }

    public AzureStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private AzureStorageException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
