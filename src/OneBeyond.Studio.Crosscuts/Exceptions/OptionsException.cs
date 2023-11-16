using System;

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
}
