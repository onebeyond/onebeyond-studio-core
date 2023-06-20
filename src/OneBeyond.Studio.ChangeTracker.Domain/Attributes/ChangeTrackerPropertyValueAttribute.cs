using EnsureThat;

namespace OneBeyond.Studio.ChangeTracker.Domain.Attributes;

/// <summary>
/// Used to replace an entity property with something else, like a placeholder.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class ChangeTrackerPropertyValueAttribute : Attribute
{
    public ChangeTrackerPropertyValueAttribute(object value)
    {
        Value = EnsureArg.IsNotNull(value, nameof(value));
    }

    public object Value { get; }
}
