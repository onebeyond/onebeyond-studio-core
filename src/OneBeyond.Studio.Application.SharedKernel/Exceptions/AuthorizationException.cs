using System;
using System.Runtime.Serialization;
using OneBeyond.Studio.Crosscuts.Exceptions;

namespace OneBeyond.Studio.Application.SharedKernel.Exceptions;

/// <summary>
/// </summary>
[Serializable]
public abstract class AuthorizationException : OneBeyondException
{
    /// <summary>
    /// </summary>
    protected AuthorizationException()
        : base()
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    protected AuthorizationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    protected AuthorizationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected AuthorizationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
