using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;
using OneBeyond.Studio.Crosscuts.Logging;
using Xunit;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.IntegrationEvents;


public sealed class IntegrationEventTypeRegistryTests
{
    static IntegrationEventTypeRegistryTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>()
            ?? throw new System.Exception("Unable to resolve ILoggerFactory interface.");
        LogManager.TryConfigure(loggerFactory);
    }

    [Fact]
    public void TestIntegrationEventTypeRegistryScansAllProperlyMarkedTypesInAssembly()
    {
        var integrationEventTypeRegistry = new IntegrationEventTypeRegistry(new[] { Assembly.GetExecutingAssembly() });

        var integrationEventTypes = integrationEventTypeRegistry.IntegrationEventTypes;

        Assert.Equal(5, integrationEventTypes.Count());
        Assert.Contains(integrationEventTypes, integrationEventType =>
            integrationEventType.TypeName == TestableIntegrationEvents.ThisHappenedTypeName
            && integrationEventType.Version == 1.1m
            && integrationEventType.ClrType == typeof(TestableIntegrationEvents.ThisHappened_1_1));
        Assert.Contains(integrationEventTypes, integrationEventType =>
            integrationEventType.TypeName == TestableIntegrationEvents.ThisHappenedTypeName
            && integrationEventType.Version == 1.2m
            && integrationEventType.ClrType == typeof(TestableIntegrationEvents.ThisHappened_1_2));
        Assert.Contains(integrationEventTypes, integrationEventType =>
            integrationEventType.TypeName == TestableIntegrationEvents.ThisHappenedTypeName
            && integrationEventType.Version == 1.4m
            && integrationEventType.ClrType == typeof(TestableIntegrationEvents.ThisHappened_1_4));
        Assert.Contains(integrationEventTypes, integrationEventType =>
            integrationEventType.TypeName == TestableIntegrationEvents.ThisHappenedTypeName
            && integrationEventType.Version == 2.1m
            && integrationEventType.ClrType == typeof(TestableIntegrationEvents.ThisHappened_2_1));
        Assert.Contains(integrationEventTypes, integrationEventType =>
            integrationEventType.TypeName == TestableIntegrationEvents.ThatHappenedTypeName
            && integrationEventType.Version == 1.0m
            && integrationEventType.ClrType == typeof(TestableIntegrationEvents.ThatHappened_1_0));
    }

    [Fact]
    public void TestIntegrationEventTypeRegistryFindsTypeByExactMatchRegardlessTypeNameCase()
    {
        var integrationEventTypeRegistry = new IntegrationEventTypeRegistry(new[] { Assembly.GetExecutingAssembly() });

        var integrationEventType = integrationEventTypeRegistry.FindIntegrationEventType(
            TestableIntegrationEvents.ThisHappenedTypeName,
            1, 4);

        Assert.NotNull(integrationEventType);
        Assert.Equal(TestableIntegrationEvents.ThisHappenedTypeName, integrationEventType!.TypeName);
        Assert.Equal(1.4m, integrationEventType.Version);

        integrationEventType = integrationEventTypeRegistry.FindIntegrationEventType(
            TestableIntegrationEvents.ThisHappenedTypeName.ToLower(),
            1, 4);

        Assert.NotNull(integrationEventType);
        Assert.Equal(TestableIntegrationEvents.ThisHappenedTypeName, integrationEventType!.TypeName);
        Assert.Equal(1.4m, integrationEventType.Version);
    }

    [Fact]
    public void TestIntegrationEventTypeRegistryFindsBackwordCompatibleType()
    {
        var integrationEventTypeRegistry = new IntegrationEventTypeRegistry(new[] { Assembly.GetExecutingAssembly() });

        var integrationEventType = integrationEventTypeRegistry.FindIntegrationEventType(
            TestableIntegrationEvents.ThisHappenedTypeName,
            1, 3);

        Assert.NotNull(integrationEventType);
        Assert.Equal(TestableIntegrationEvents.ThisHappenedTypeName, integrationEventType!.TypeName);
        Assert.Equal(1.2m, integrationEventType.Version);
    }

    [Fact]
    public void TestIntegrationEventTypeRegistryDoesNotFindTypeForALowerVersionWhenAllRegisteredTypesAreNewer()
    {
        var integrationEventTypeRegistry = new IntegrationEventTypeRegistry(new[] { Assembly.GetExecutingAssembly() });

        var integrationEventType = integrationEventTypeRegistry.FindIntegrationEventType(
            TestableIntegrationEvents.ThisHappenedTypeName,
            1, 0);

        Assert.Null(integrationEventType);
    }

    [Fact]
    public void TestIntegrationEventTypeRegistryDoesNotFindTypeForALowerVersionWhenThereAreBackwordIncompatibleTypes()
    {
        var integrationEventTypeRegistry = new IntegrationEventTypeRegistry(new[] { Assembly.GetExecutingAssembly() });

        var integrationEventType = integrationEventTypeRegistry.FindIntegrationEventType(
            TestableIntegrationEvents.ThisHappenedTypeName,
            2, 0);

        Assert.Null(integrationEventType);
    }
}


