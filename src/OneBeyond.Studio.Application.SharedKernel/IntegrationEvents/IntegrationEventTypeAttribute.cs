using System;
using System.Reflection;
using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;

namespace OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

/// <summary>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class IntegrationEventTypeAttribute : Attribute
{
    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    public IntegrationEventTypeAttribute(string name)
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));

        Name = name;
    }

    /// <summary>
    /// </summary>
    public string Name { get; }

    internal static string GetName(Type integrationEventClrType)
    {
        EnsureArg.IsNotNull(integrationEventClrType, nameof(integrationEventClrType));

        return integrationEventClrType.GetCustomAttribute<IntegrationEventTypeAttribute>(true)?.Name
            ?? throw new IntegrationEventException(
                $"Integration event of {integrationEventClrType.Name} type does not have"
                + $" {nameof(IntegrationEventTypeAttribute)} attribute associated with it.");
    }
}
