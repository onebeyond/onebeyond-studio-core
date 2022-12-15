using System;
using EnsureThat;

namespace OneBeyond.Studio.EntityAuditing.Domain.Attributes;

/// <summary>
/// Used to replace an entity property with something else, like a placeholder.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class AuditPropertyValueAttribute : Attribute
{
    public AuditPropertyValueAttribute(object value)
    {
        EnsureArg.IsNotNull(value, nameof(value));
        Value = value;
    }

    public object Value { get; }
}
