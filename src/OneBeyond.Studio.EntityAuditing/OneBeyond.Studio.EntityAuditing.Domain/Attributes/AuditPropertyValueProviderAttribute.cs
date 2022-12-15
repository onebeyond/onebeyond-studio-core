using System;
using EnsureThat;

namespace OneBeyond.Studio.EntityAuditing.Domain.Attributes;

/// <summary>
/// Used to replace an entity property value with something else defined in a method resolved by DI.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class AuditPropertyValueProviderAttribute : Attribute
{
    public AuditPropertyValueProviderAttribute(Type type)
    {
        EnsureArg.IsNotNull(type, nameof(type));
        var implementsInterface = typeof(IAuditPropertyValueProvider).IsAssignableFrom(type) && type.IsClass;
        if (!implementsInterface)
        {
            throw new ArgumentException($"The provided type must be a class implementing {nameof(IAuditPropertyValueProvider)} interface");
        }

        Type = type;
    }

    public Type Type { get; }
}
