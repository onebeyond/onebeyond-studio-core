using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;
using EnsureThat;
using Microsoft.AspNetCore.Http;

namespace OneBeyond.Studio.Hosting.AspNet.Http;

public static class HttpContextAccessorExtensions
{
    public static bool TryGetUserClaimValue(
        this IHttpContextAccessor httpContextAccessor, 
        string claimType,
        [NotNullWhen(true)] out string? value)
    {
        EnsureArg.IsNotNull(httpContextAccessor, nameof(httpContextAccessor));
        EnsureArg.IsNotNullOrWhiteSpace(claimType, nameof(claimType));

        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Current execution context is not bound to any HTTP request.");

        var userIdentity = httpContext.User.Identity
            ?? throw new InvalidOperationException("Current user is not identified.");

        value = TryGetClaimFromIdentity(userIdentity, claimType);
        return value != null;
    }

    public static string GetUserClaimValue(
        this IHttpContextAccessor httpContextAccessor, 
        string claimType)
    {
        EnsureArg.IsNotNull(httpContextAccessor, nameof(httpContextAccessor));
        EnsureArg.IsNotNullOrWhiteSpace(claimType, nameof(claimType));

        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Current execution context is not bound to any HTTP request.");

        var userIdentity = httpContext.User.Identity
            ?? throw new InvalidOperationException("Current user is not identified.");

        return GetClaimFromIdentity(userIdentity, claimType);
    }

    [Obsolete("This method will soon be deleted, please use GetUserClaimValue instead.")]
    public static string GetUserAuthId(
        this IHttpContextAccessor httpContextAccessor, 
        string claimType)
        => GetUserClaimValue(httpContextAccessor, claimType);

    private static string GetClaimFromIdentity(
        IIdentity identity, 
        string type)
    {
        var claimIdentity = identity as ClaimsIdentity
            ?? throw new InvalidOperationException("Current user does not contain claims.");
        var claim = claimIdentity.FindFirst(type)
            ?? throw new InvalidOperationException($"Current user does not contain claim for {type}.");
        return claim.Value;
    }

    private static string? TryGetClaimFromIdentity(
        IIdentity identity, 
        string type) 
        => (identity as ClaimsIdentity)?.FindFirst(type)?.Value;
}
