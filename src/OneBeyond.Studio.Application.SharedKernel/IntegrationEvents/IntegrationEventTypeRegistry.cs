using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnsureThat;
using Microsoft.Extensions.Logging;
using MoreLinq;
using OneBeyond.Studio.Crosscuts.Comparers;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;

namespace OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

internal sealed class IntegrationEventTypeRegistry
{
    private static readonly ILogger Logger = LogManager.CreateLogger<IntegrationEventTypeRegistry>();
    private static readonly Type TypeOfIntegrationEvent = typeof(IntegrationEvent);
    private static readonly IComparer<IntegrationEventType> IntegrationEventTypeByVersionComparer =
        Comparer.Create<IntegrationEventType>(
            (integrationEventType1, integrationEventType2) =>
                decimal.Compare(integrationEventType1!.Version, integrationEventType2!.Version));

    private readonly IReadOnlyDictionary<string, List<IntegrationEventType>> _integrationEventTypes;

    public IntegrationEventTypeRegistry(IReadOnlyCollection<Assembly> integrationEventAssemblies)
    {
        EnsureArg.IsNotNull(integrationEventAssemblies, nameof(integrationEventAssemblies));

        _integrationEventTypes = ExtractIntegrationEventTypes(integrationEventAssemblies);
    }

    internal IReadOnlyCollection<IntegrationEventType> IntegrationEventTypes =>
        _integrationEventTypes.Values
            .SelectMany((integrationEventType) => integrationEventType)
            .ToArray();

    public IntegrationEventType? FindIntegrationEventType(string typeName, int majorVersion, int minorVersion)
    {
        EnsureArg.IsNotNullOrWhiteSpace(typeName, nameof(typeName));

        if (!_integrationEventTypes.TryGetValue(typeName.ToLower(), out var integrationEventTypesByName))
        {
            return default;
        }

        var integrationEventTypeIdx = integrationEventTypesByName.BinarySearch(
            new IntegrationEventType(typeName, majorVersion, minorVersion, TypeOfIntegrationEvent),
            IntegrationEventTypeByVersionComparer);

        if (integrationEventTypeIdx < 0)
        {
            integrationEventTypeIdx = ~integrationEventTypeIdx - 1;
        }

        if (integrationEventTypeIdx < 0)
        {
            return default;
        }

        var integrationEventType = integrationEventTypesByName[integrationEventTypeIdx];

        return integrationEventType.MajorVersion == majorVersion
            ? integrationEventType
            : default;
    }

    private static IReadOnlyDictionary<string, List<IntegrationEventType>> ExtractIntegrationEventTypes(
        IReadOnlyCollection<Assembly> integrationEventAssemblies)
    {
        var integrationEventTypes = integrationEventAssemblies
            .SelectMany((assembly) => assembly.GetTypes())
            .Where((type) => !type.IsAbstract && TypeOfIntegrationEvent.IsAssignableFrom(type))
            .Aggregate(
                new Dictionary<string, List<IntegrationEventType>>(),
                (result, integrationEventClrType) =>
                {
                    var integrationEventTypeName =
                        IntegrationEventTypeAttribute.GetName(integrationEventClrType);

                    var (integrationEventMajorVersion, integrationEventMinorVersion) =
                        IntegrationEventVersionAttribute.GetVersion(integrationEventClrType);

                    var integrationEventTypeKey = integrationEventTypeName.ToLower();

                    Logger.LogInformation(
                        "Discovered integration event of {IntegrationEventTypeName} type and version"
                        + " {IntegrationEventVersion} built on {IntegrationEventClrType} CLR type",
                        integrationEventTypeName,
                        IntegrationEventVersion.ToString(integrationEventMajorVersion, integrationEventMinorVersion),
                        integrationEventClrType.AssemblyQualifiedName);

                    if (!result.TryGetValue(integrationEventTypeKey, out var integrationEventTypesByName))
                    {
                        integrationEventTypesByName = new List<IntegrationEventType>();
                        result.Add(integrationEventTypeKey, integrationEventTypesByName);
                    }

                    var integrationEventType = integrationEventTypesByName
                        .FirstOrDefault((integrationEventType) =>
                            integrationEventType.MajorVersion == integrationEventMajorVersion
                            && integrationEventType.MinorVersion == integrationEventMinorVersion);

                    if (integrationEventType is null)
                    {
                        integrationEventTypesByName.Add(
                            new IntegrationEventType(
                                integrationEventTypeName,
                                integrationEventMajorVersion,
                                integrationEventMinorVersion,
                                integrationEventClrType));
                    }
                    else if (integrationEventType.ClrType != integrationEventClrType)
                    {
                        throw new IntegrationEventException(
                            $"Integration event of {integrationEventTypeName} type and version"
                            + $" {IntegrationEventVersion.ToString(integrationEventMajorVersion, integrationEventMinorVersion)}"
                            + $" built on {integrationEventClrType.AssemblyQualifiedName} CLR type"
                            + $" conflicts with already registered one built on {integrationEventType.ClrType.AssemblyQualifiedName} CLR type.");
                    }

                    return result;
                });

        integrationEventTypes.Values
            .ForEach((integrationEventTypesByName) => integrationEventTypesByName.Sort(IntegrationEventTypeByVersionComparer));

        return integrationEventTypes;
    }
}
