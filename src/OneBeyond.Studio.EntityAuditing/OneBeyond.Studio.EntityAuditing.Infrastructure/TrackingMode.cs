namespace OneBeyond.Studio.EntityAuditing.Infrastructure;

// NOTE: we cannot use SmartEnum here as this is used as part
// of an option object that must be readable from appsettings.json

/// <summary>
/// Auditing Tracking Mode
/// </summary>
public enum TrackingMode
{
    /// <summary>
    /// No entity is tracked by default, except those explicitly included.
    /// </summary>
    TrackOnlyIncluded = 1,

    /// <summary>
    /// All the entities are tracked by default, except those explicitly ignored. (Default)
    /// </summary>
    TrackAllButIgnored = 2
}
