using System;
using OneBeyond.Studio.Crosscuts.Exceptions;

namespace OneBeyond.Studio.Application.SharedKernel.Exceptions;

/// <summary>
/// </summary>
[Serializable]
public sealed class IntegrationEventException : OneBeyondException
{
    internal IntegrationEventException()
        : base()
    {
    }

    internal IntegrationEventException(string message)
        : base(message)
    {
    }

    internal IntegrationEventException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
