using EnsureThat;

namespace OneBeyond.Studio.ChangeTracker.Domain.Attributes;


/// <summary>
/// Used to define a custom name for the entity class
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ChangeTrackerEntityNameAttribute : Attribute
{
    public ChangeTrackerEntityNameAttribute(string name)
    {
        Name = EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
    }

    public string Name { get; }
}
