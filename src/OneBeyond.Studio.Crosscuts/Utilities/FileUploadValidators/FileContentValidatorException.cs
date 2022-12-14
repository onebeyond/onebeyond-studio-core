using System;
using System.Runtime.Serialization;
using OneBeyond.Studio.Crosscuts.Exceptions;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

/// <summary>
/// </summary>
[Serializable]
public sealed class FileContentValidatorException : OneBeyondException
{
    /// <summary>
    /// </summary>
    private FileContentValidatorException()
        : base()
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    public FileContentValidatorException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public FileContentValidatorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    private FileContentValidatorException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
