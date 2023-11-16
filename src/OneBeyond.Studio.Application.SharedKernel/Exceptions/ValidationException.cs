using System;
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
}
