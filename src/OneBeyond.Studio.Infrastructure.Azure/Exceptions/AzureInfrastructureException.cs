using System;
using OneBeyond.Studio.Crosscuts.Exceptions;

namespace OneBeyond.Studio.Infrastructure.Azure.Exceptions;

[Serializable]
public class AzureInfrastructureException : OneBeyondException
{
    public AzureInfrastructureException()
    {
    }

    public AzureInfrastructureException(string message)
        : base(message)
    {
    }

    public AzureInfrastructureException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
