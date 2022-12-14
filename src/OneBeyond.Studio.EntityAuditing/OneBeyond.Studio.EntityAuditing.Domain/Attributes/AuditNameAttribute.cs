using System;
using EnsureThat;

namespace OneBeyond.Studio.EntityAuditing.Domain.Attributes;

/// <summary>
/// Used to define a custom name for the entity class
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AuditNameAttribute : Attribute
{
    public AuditNameAttribute(string name)
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public string Name { get; }
}
