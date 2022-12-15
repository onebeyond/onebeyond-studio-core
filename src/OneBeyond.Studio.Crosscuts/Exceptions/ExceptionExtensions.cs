using System;
using System.Threading;

namespace OneBeyond.Studio.Crosscuts.Exceptions;

/// <summary>
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Indicates whether the specified exception in either of <see cref="OutOfMemoryException"/>,
    /// <see cref="StackOverflowException"/>, or <see cref="ThreadAbortException"/>. These exceptions must never be
    /// suppressed.
    /// </summary>
    public static bool IsCritical(this Exception exception)
    {
        return exception is StackOverflowException
            || exception is OutOfMemoryException
            || exception is ThreadAbortException;
    }
}
