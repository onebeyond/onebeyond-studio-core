using System;
using System.Reflection;
using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;

namespace OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

/// <summary>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IntegrationEventVersionAttribute : Attribute
{
    /// <summary>
    /// </summary>
    /// <param name="major"></param>
    /// <param name="minor"></param>
    public IntegrationEventVersionAttribute(int major, int minor)
    {
        EnsureArg.IsGt(major, 0, nameof(major));
        EnsureArg.IsGte(minor, 0, nameof(minor));

        Major = major;
        Minor = minor;
    }

    /// <summary>
    /// </summary>
    public int Major { get; }
    /// <summary>
    /// </summary>
    public int Minor { get; }

    internal static (int Major, int Minor) GetVersion(Type integrationEventClrType)
    {
        EnsureArg.IsNotNull(integrationEventClrType, nameof(integrationEventClrType));

        var integrationEventVersion = integrationEventClrType.GetCustomAttribute<IntegrationEventVersionAttribute>(false)
            ?? throw new IntegrationEventException(
                $"Integration event of {integrationEventClrType.Name} type does not have"
                + $" {nameof(IntegrationEventVersionAttribute)} attribute associated with it.");

        return (integrationEventVersion.Major, integrationEventVersion.Minor);
    }
}
