namespace OneBeyond.Studio.EntityAuditing.Infrastructure.Options;

public sealed class EntityAuditingOptions
{
    /// <summary>
    /// Set this to true if you do not want
    /// to save entity changes at all
    /// Even if set to true, you can still use the readers
    /// </summary>
    public bool OmitChangeWriting { get; init; }

    /// <summary>
    /// Set the tracking mode between:<br /> 
    /// <see cref="TrackingMode.TrackAllButIgnored"/>: All the entities are tracked by default, except those explicitly ignored. (Default)<br /> 
    /// <see cref="TrackingMode.TrackOnlyIncluded"/>: No entity is tracked by default, except those explicitly included
    /// </summary>
    public TrackingMode TrackingMode { get; init; } = TrackingMode.TrackAllButIgnored;

    /// <summary>
    /// When using bulk insert, all aduti entries will be saves in one go, instead of saving those one by one
    /// </summary>
    public bool UseBulkInsert { get; init; } = false;
}
