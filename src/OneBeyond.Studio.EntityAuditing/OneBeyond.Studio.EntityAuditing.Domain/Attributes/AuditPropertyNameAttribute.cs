using System;
using EnsureThat;

namespace OneBeyond.Studio.EntityAuditing.Domain.Attributes;

/// <summary>
/// Used to define a custom name for a property of the entity
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class AuditPropertyNameAttribute : Attribute
{
    public AuditPropertyNameAttribute(string name)
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public string Name { get; }
}
