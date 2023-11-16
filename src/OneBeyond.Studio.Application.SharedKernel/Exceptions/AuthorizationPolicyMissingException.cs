using System;

namespace OneBeyond.Studio.Application.SharedKernel.Exceptions;

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

    /// <summary>
    /// </summary>
    public Type RequestType { get; }
}
