using System;
using System.Runtime.Serialization;

namespace OneBeyond.Studio.Crosscuts.Exceptions;

/// <summary>
/// </summary>
[Serializable]
public sealed class OptionsException : OneBeyondException
{
    /// <summary>
    /// </summary>
    public OptionsException()
        : base()
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    public OptionsException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public OptionsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    private OptionsException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
