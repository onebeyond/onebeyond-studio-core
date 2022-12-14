using System;
using System.Runtime.Serialization;

namespace OneBeyond.Studio.Crosscuts.Exceptions;

[Serializable]
public abstract class OneBeyondException : Exception
{
    /// <summary>
    /// </summary>
    protected OneBeyondException()
        : base()
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    protected OneBeyondException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    protected OneBeyondException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected OneBeyondException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
