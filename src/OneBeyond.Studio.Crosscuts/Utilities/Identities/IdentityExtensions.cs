using System.Security.Claims;
using System.Security.Principal;
using EnsureThat;

namespace OneBeyond.Studio.Crosscuts.Utilities.Identities;

/// <summary>
/// </summary>
public static class IdentityExtensions
{
    /// <summary>
    /// Tries to get a user login id as string from the identity.
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>Either found user login id or null</returns>
    public static string? TryGetLoginId(this IIdentity identity)
    {
        EnsureArg.IsNotNull(identity, nameof(identity));

        return GetFromClaim(identity, ClaimTypes.NameIdentifier);
    }

    private static string? GetFromClaim(IIdentity identity, string type)
    {
        if (identity is ClaimsIdentity cIdentity)
        {
            var claim = cIdentity.FindFirst(type);
            if (claim != null)
            {
                return claim.Value;
            }
        }
        return null;
    }
}
