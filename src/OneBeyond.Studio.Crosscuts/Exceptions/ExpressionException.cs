using System;

namespace OneBeyond.Studio.Crosscuts.Exceptions;

/// <summary>
/// </summary>
[Serializable]
public sealed class ExpressionException : OneBeyondException
{
    internal ExpressionException()
        : base()
    {
    }

    internal ExpressionException(string message)
        : base(message)
    {
    }

    internal ExpressionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
