using System;
using System.Runtime.Serialization;
using OneBeyond.Studio.Crosscuts.Exceptions;

namespace OneBeyond.Studio.Application.SharedKernel.Exceptions;

/// <summary>
/// Validation exception
/// </summary>
[Serializable]
public class ValidationException : OneBeyondException
{
    /// <summary>
    /// </summary>
    public ValidationException()
        : base()
    {
    }

    /// <summary>
    /// </summary>
    public ValidationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// </summary>
    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected ValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

}
