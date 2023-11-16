using System;
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
}
