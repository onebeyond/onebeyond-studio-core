using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;
using OneBeyond.Studio.Crosscuts.Logging;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.IntegrationEvents;

[TestClass]
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

    [TestMethod]
    public void TestIntegrationEventTypeRegistryScansAllProperlyMarkedTypesInAssembly()
    {
        var integrationEventTypeRegistry = new IntegrationEventTypeRegistry(new[] { Assembly.GetExecutingAssembly() });

        var integrationEventTypes = integrationEventTypeRegistry.IntegrationEventTypes;

        Assert.AreEqual(integrationEventTypes.Count, 5);
        Assert.IsTrue(integrationEventTypes.Any((integrationEventType) =>
            integrationEventType.TypeName == TestableIntegrationEvents.ThisHappenedTypeName
            && integrationEventType.Version == 1.1m
            && integrationEventType.ClrType == typeof(TestableIntegrationEvents.ThisHappened_1_1)));
        Assert.IsTrue(integrationEventTypes.Any((integrationEventType) =>
            integrationEventType.TypeName == TestableIntegrationEvents.ThisHappenedTypeName
            && integrationEventType.Version == 1.2m
            && integrationEventType.ClrType == typeof(TestableIntegrationEvents.ThisHappened_1_2)));
        Assert.IsTrue(integrationEventTypes.Any((integrationEventType) =>
            integrationEventType.TypeName == TestableIntegrationEvents.ThisHappenedTypeName
            && integrationEventType.Version == 1.4m
            && integrationEventType.ClrType == typeof(TestableIntegrationEvents.ThisHappened_1_4)));
        Assert.IsTrue(integrationEventTypes.Any((integrationEventType) =>
            integrationEventType.TypeName == TestableIntegrationEvents.ThisHappenedTypeName
            && integrationEventType.Version == 2.1m
            && integrationEventType.ClrType == typeof(TestableIntegrationEvents.ThisHappened_2_1)));
        Assert.IsTrue(integrationEventTypes.Any((integrationEventType) =>
            integrationEventType.TypeName == TestableIntegrationEvents.ThatHappenedTypeName
            && integrationEventType.Version == 1.0m
            && integrationEventType.ClrType == typeof(TestableIntegrationEvents.ThatHappened_1_0)));
    }

    [TestMethod]
    public void TestIntegrationEventTypeRegistryFindsTypeByExactMatchRegardlessTypeNameCase()
    {
        var integrationEventTypeRegistry = new IntegrationEventTypeRegistry(new[] { Assembly.GetExecutingAssembly() });

        var integrationEventType = integrationEventTypeRegistry.FindIntegrationEventType(
            TestableIntegrationEvents.ThisHappenedTypeName,
            1, 4);

        Assert.IsNotNull(integrationEventType);
        Assert.AreEqual(integrationEventType!.TypeName, TestableIntegrationEvents.ThisHappenedTypeName);
        Assert.AreEqual(integrationEventType.Version, 1.4m);

        integrationEventType = integrationEventTypeRegistry.FindIntegrationEventType(
            TestableIntegrationEvents.ThisHappenedTypeName.ToLower(),
            1, 4);

        Assert.IsNotNull(integrationEventType);
        Assert.AreEqual(integrationEventType!.TypeName, TestableIntegrationEvents.ThisHappenedTypeName);
        Assert.AreEqual(integrationEventType.Version, 1.4m);
    }

    [TestMethod]
    public void TestIntegrationEventTypeRegistryFindsBackwordCompatibleType()
    {
        var integrationEventTypeRegistry = new IntegrationEventTypeRegistry(new[] { Assembly.GetExecutingAssembly() });

        var integrationEventType = integrationEventTypeRegistry.FindIntegrationEventType(
            TestableIntegrationEvents.ThisHappenedTypeName,
            1, 3);

        Assert.IsNotNull(integrationEventType);
        Assert.AreEqual(integrationEventType!.TypeName, TestableIntegrationEvents.ThisHappenedTypeName);
        Assert.AreEqual(integrationEventType.Version, 1.2m);
    }

    [TestMethod]
    public void TestIntegrationEventTypeRegistryDoesNotFindTypeForALowerVersionWhenAllRegisteredTypesAreNewer()
    {
        var integrationEventTypeRegistry = new IntegrationEventTypeRegistry(new[] { Assembly.GetExecutingAssembly() });

        var integrationEventType = integrationEventTypeRegistry.FindIntegrationEventType(
            TestableIntegrationEvents.ThisHappenedTypeName,
            1, 0);

        Assert.IsNull(integrationEventType);
    }

    [TestMethod]
    public void TestIntegrationEventTypeRegistryDoesNotFindTypeForALowerVersionWhenThereAreBackwordIncompatibleTypes()
    {
        var integrationEventTypeRegistry = new IntegrationEventTypeRegistry(new[] { Assembly.GetExecutingAssembly() });

        var integrationEventType = integrationEventTypeRegistry.FindIntegrationEventType(
            TestableIntegrationEvents.ThisHappenedTypeName,
            2, 0);

        Assert.IsNull(integrationEventType);
    }
}
