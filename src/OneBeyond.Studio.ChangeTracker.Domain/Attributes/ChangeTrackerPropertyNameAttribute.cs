using EnsureThat;

namespace OneBeyond.Studio.ChangeTracker.Domain.Attributes;

/// <summary>
/// Used to define a custom name for a property of the entity
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class ChangeTrackerPropertyNameAttribute : Attribute
{
    public ChangeTrackerPropertyNameAttribute(string name)
    {
        Name = EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
    }

    public string Name { get; }
}
