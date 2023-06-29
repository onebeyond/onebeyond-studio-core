using OneBeyond.Studio.Application.SharedKernel.AmbientContexts;

namespace OneBeyond.Studio.FeaturePermissions.AmbientContexts;
public static class AmbientContextExtensions
{
    /// <summary>
    /// Returns true if the user has the feature passed in, false otherwise.
    /// </summary>
    public static bool UserIsInGroup<TFeaturePermissionAmbientContext>(this IAmbientContextAccessor<TFeaturePermissionAmbientContext> ambientContextAccessor, string group) 
        where TFeaturePermissionAmbientContext : FeaturePermissionAmbientContext
        => ambientContextAccessor.UserIsInAnyGroup(new[] { group });

    /// <summary>
    /// Returns true if the user has any of the features passed into the method, false otherwise.
    /// </summary>
    public static bool UserIsInAnyGroup<TFeaturePermissionAmbientContext>(this IAmbientContextAccessor<TFeaturePermissionAmbientContext> ambientContextAccessor, IEnumerable<string> features)
        where TFeaturePermissionAmbientContext : FeaturePermissionAmbientContext
    {
        return GetFeaturePermissions(ambientContextAccessor).Select(x => x.ToLower()).Any(x => features.Contains(x));
    }

    /// <summary>
    /// Returns true if the user has all of the features passed into the method, false otherwise.
    /// </summary>
    public static bool UserIsInAllGroups<TFeaturePermissionAmbientContext>(this IAmbientContextAccessor<TFeaturePermissionAmbientContext> ambientContextAccessor, IEnumerable<string> features)
        where TFeaturePermissionAmbientContext : FeaturePermissionAmbientContext
    {
        return GetFeaturePermissions(ambientContextAccessor).Select(x => x.ToLower()).Intersect(features).Count() == features.Count();
    }

    private static IEnumerable<string> GetFeaturePermissions<TFeaturePermissionAmbientContext>(IAmbientContextAccessor<TFeaturePermissionAmbientContext> ambientContextAccessor)
        where TFeaturePermissionAmbientContext : FeaturePermissionAmbientContext
    {
        var featurePermissions = ambientContextAccessor
                .AmbientContext
                .UserContext?
                .FeaturePermissions;

        if (featurePermissions is null)
        {
            throw new FeaturePermissionException("No feature permissions were accessible.");
        }

        return featurePermissions;
    }
}
