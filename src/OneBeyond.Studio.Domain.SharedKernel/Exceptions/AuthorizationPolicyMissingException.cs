using System;
using System.Runtime.Serialization;

namespace OneBeyond.Studio.Domain.SharedKernel.Exceptions;

/// <summary>
/// </summary>
[Serializable]
public sealed class AuthorizationPolicyMissingException : AuthorizationException
{
    /// <summary>
    /// </summary>
    /// <param name="requestType"></param>
    public AuthorizationPolicyMissingException(Type requestType)
        : base($"Request of the {requestType.FullName} type does not have any authorization policy assigned to it.")
    {
        RequestType = requestType;
    }

    private AuthorizationPolicyMissingException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        RequestType = null!;
    }

    /// <summary>
    /// </summary>
    public Type RequestType { get; }
}
