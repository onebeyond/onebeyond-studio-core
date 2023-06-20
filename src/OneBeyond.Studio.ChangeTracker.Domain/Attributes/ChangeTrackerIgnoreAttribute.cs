namespace OneBeyond.Studio.ChangeTracker.Domain.Attributes;

/// <summary>
/// Can be used to ignore an entity (class) on the Audit logs. 
/// Also can be used to ignore entity properties.
/// Note! This property is not inherited, so if you set it for a base class - it will not be applied to derived classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ChangeTrackerIgnoreAttribute : Attribute
{
}
