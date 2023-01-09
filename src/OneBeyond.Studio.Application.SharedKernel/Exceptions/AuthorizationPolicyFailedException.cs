using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OneBeyond.Studio.Domain.SharedKernel.Authorization;

namespace OneBeyond.Studio.Application.SharedKernel.Exceptions;

/// <summary>
/// </summary>
[Serializable]
public sealed class AuthorizationPolicyFailedException : AuthorizationException
{
    /// <summary>
    /// </summary>
    /// <param name="policy"></param>
    /// <param name="requestType"></param>
    /// <param name="exceptions"></param>
    public AuthorizationPolicyFailedException(
        AuthorizationPolicyAttribute policy,
        Type requestType,
        IEnumerable<Exception> exceptions)
        : base(
              $"Neither of the authorization policy requirements: "
            + $"'{string.Join(", ", policy.RequirementTypes.Select((requirementType) => requirementType.Key.FullName))}' "
            + $"are met on request '{requestType.FullName}'.",
              new AggregateException(exceptions))
    {
        Policy = policy;
        RequestType = requestType;
        Exceptions = exceptions;
    }

    private AuthorizationPolicyFailedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        Policy = null!;
        RequestType = null!;
        Exceptions = null!;
    }

    /// <summary>
    /// </summary>
    public AuthorizationPolicyAttribute Policy { get; }
    /// <summary>
    /// </summary>
    public Type RequestType { get; }
    /// <summary>
    /// </summary>
    public IEnumerable<Exception> Exceptions { get; }
}
