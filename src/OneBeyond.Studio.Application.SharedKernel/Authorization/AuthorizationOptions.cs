namespace OneBeyond.Studio.Application.SharedKernel.Authorization;

/// <summary>
/// </summary>
public sealed record AuthorizationOptions
{
    /// <summary>
    /// Requests without authroization policies bound to them can be passed for further execution.
    /// </summary>
    public bool AllowUnattributedRequests { get; set; }
}
