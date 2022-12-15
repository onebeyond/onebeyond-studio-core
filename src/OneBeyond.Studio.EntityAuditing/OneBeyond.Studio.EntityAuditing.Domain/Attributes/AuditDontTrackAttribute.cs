using System;

namespace OneBeyond.Studio.EntityAuditing.Domain.Attributes;

/// <summary>
/// Used to ignore an entity (class) on the Audit logs. Also can be used to ignore entity properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class AuditDontTrackAttribute : Attribute
{
}
