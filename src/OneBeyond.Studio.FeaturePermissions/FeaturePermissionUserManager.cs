using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OneBeyond.Studio.FeaturePermissions;

/// <summary>
/// User Manager that supports feature permissions
/// </summary>
/// <typeparam name="TAuthUser"></typeparam>
public class FeaturePermissionUserManager<TAuthUser> : UserManager<TAuthUser>
    where TAuthUser : IdentityUser<Guid>
{
    private readonly ClaimComparer _claimComparer = new ClaimComparer(new ClaimComparer.Options() { IgnoreIssuer = true });
    private readonly SignInManager<TAuthUser> _signInManager;
    public FeaturePermissionUserManager(
        IUserStore<TAuthUser> store, 
        IOptions<IdentityOptions> optionsAccessor, 
        IPasswordHasher<TAuthUser> passwordHasher, 
        IEnumerable<IUserValidator<TAuthUser>> userValidators, 
        IEnumerable<IPasswordValidator<TAuthUser>> passwordValidators, 
        ILookupNormalizer keyNormalizer, 
        IdentityErrorDescriber errors, 
        IServiceProvider services, 
        ILogger<UserManager<TAuthUser>> logger,
        SignInManager<TAuthUser> signInManager) : 
        base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _signInManager = signInManager;
    }

    /// <summary>
    /// Sets the exact set of feature permissions for the user. Permissions not in the featurePermissions parameter
    /// will be removed. These will be added as ApplicationClaims.ApplicationFeature claims.
    /// </summary>
    /// <param name="user">User to set feature permissions for</param>
    /// <param name="featurePermissions">Exact list of feature permissions as strings</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SetFeaturePermissionClaimsAsync(TAuthUser user, IEnumerable<string> featurePermissions, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(user, nameof(user));
        EnsureArg.IsNotNull(featurePermissions, nameof(featurePermissions));        

        var claimPermissions = featurePermissions.Select(x => new Claim(ApplicationClaims.ApplicationFeature, x));
        cancellationToken.ThrowIfCancellationRequested();
        await SetFeaturePermissionClaimsAsync(user, claimPermissions, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the exact set of feature permissions for the user. Permissions not in the featurePermissions parameter
    /// will be removed. All claims must be of type ApplicationClaims.ApplicationFeature
    /// </summary>
    /// <param name="user">User to set feature permissions for</param>
    /// <param name="featurePermissions">Exact list of feature permissions as Claims - all claims must be of type ApplicationClaims.ApplicationFeature</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SetFeaturePermissionClaimsAsync(TAuthUser user, IEnumerable<Claim> featurePermissions, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(user, nameof(user));
        EnsureArg.IsNotNull(featurePermissions, nameof(featurePermissions));        

        if (featurePermissions.Any(x => x.Type != ApplicationClaims.ApplicationFeature))
        {
            throw new FeaturePermissionException("All claims must be of type ApplicationClaims.ApplicationFeature");
        }

        var claims = await GetClaimsAsync(user).ConfigureAwait(false);

        var featureClaims = claims.Where(claim => claim.Type == ApplicationClaims.ApplicationFeature);

        var removedClaims = featureClaims.Except(featurePermissions, _claimComparer);

        cancellationToken.ThrowIfCancellationRequested();

        await RemoveClaimsAsync(user, removedClaims).ConfigureAwait(false);

        var addedClaims = featurePermissions.Except(featureClaims, _claimComparer);

        await AddClaimsAsync(user, addedClaims).ConfigureAwait(false);

        if (removedClaims.Any() || addedClaims.Any())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _signInManager.RefreshSignInAsync(user).ConfigureAwait(false);    
        }
    }
 }
