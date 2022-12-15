using System;
using System.Runtime.Serialization;
using OneBeyond.Studio.Crosscuts.Exceptions;

namespace OneBeyond.Studio.Domain.SharedKernel.Exceptions;

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

    private IntegrationEventException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
