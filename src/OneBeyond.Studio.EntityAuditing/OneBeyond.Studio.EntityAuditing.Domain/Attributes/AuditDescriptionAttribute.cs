using System;

namespace OneBeyond.Studio.EntityAuditing.Domain.Attributes;

/// <summary>
/// Denotes a property containing a brief meaningful description of the entity instance
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class AuditDescriptionAttribute : Attribute
{
}
