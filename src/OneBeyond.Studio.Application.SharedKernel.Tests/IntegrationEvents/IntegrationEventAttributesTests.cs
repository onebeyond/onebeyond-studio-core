using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;
using OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.IntegrationEvents;

[TestClass]
public sealed class IntegrationEventAttributesTests
{
    [TestMethod]
    public void TestIntegrationEventTypeAttributeReturnsTypeNameWhenTypeIsAttributed()
    {
        var typeName = IntegrationEventTypeAttribute.GetName(typeof(TestableIntegrationEvents.ThisHappened_1_1));

        Assert.AreEqual(TestableIntegrationEvents.ThisHappenedTypeName, typeName);
    }

    [TestMethod]
    public void TestIntegrationEventTypeAttributeReturnsTypeNameWhenTypeIsDerivedFromAttributedType()
    {
        var typeName = IntegrationEventTypeAttribute.GetName(typeof(TestableIntegrationEvents.ThisHappened_1_4));

        Assert.AreEqual(TestableIntegrationEvents.ThisHappenedTypeName, typeName);
    }

    [TestMethod]
    public void TestIntegrationEventTypeAttributeReturnsOverridenTypeName()
    {
        var typeName1 = IntegrationEventTypeAttribute.GetName(typeof(TestableIntegrationEvents.FakeHappened_1_0));
        var typeName2 = IntegrationEventTypeAttribute.GetName(typeof(TestableIntegrationEvents.FakeHappened_1_2));

        Assert.AreEqual(TestableIntegrationEvents.ThisHappenedTypeName, typeName1);
        Assert.AreEqual(TestableIntegrationEvents.ThatHappenedTypeName, typeName2);
        Assert.IsTrue(typeof(TestableIntegrationEvents.FakeHappened_1_0).IsAssignableFrom(typeof(TestableIntegrationEvents.FakeHappened_1_2)));
    }

    [TestMethod]
    public void TestIntegrationEventVersionAttributeReturnsVersionWhenTypeIsAttributed()
    {
        var version = IntegrationEventVersionAttribute.GetVersion(typeof(TestableIntegrationEvents.ThisHappened_1_1));

        Assert.AreEqual((1, 1), version);
    }

    [TestMethod]
    public void TestIntegrationEventVersionAttributeThrowsWhenTypeIsNotExplicitlyAttributed()
    {
        Assert.Throws<IntegrationEventException>(() => IntegrationEventVersionAttribute.GetVersion(typeof(TestableIntegrationEvents.FakeHappened_1_1)));        
    }
}
