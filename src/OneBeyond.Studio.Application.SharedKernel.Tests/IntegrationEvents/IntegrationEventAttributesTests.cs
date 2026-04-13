using Xunit;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;
using OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.IntegrationEvents;


public sealed class IntegrationEventAttributesTests
{
    [Fact]
    public void TestIntegrationEventTypeAttributeReturnsTypeNameWhenTypeIsAttributed()
    {
        var typeName = IntegrationEventTypeAttribute.GetName(typeof(TestableIntegrationEvents.ThisHappened_1_1));

        Assert.Equal(TestableIntegrationEvents.ThisHappenedTypeName, typeName);
    }

    [Fact]
    public void TestIntegrationEventTypeAttributeReturnsTypeNameWhenTypeIsDerivedFromAttributedType()
    {
        var typeName = IntegrationEventTypeAttribute.GetName(typeof(TestableIntegrationEvents.ThisHappened_1_4));

        Assert.Equal(TestableIntegrationEvents.ThisHappenedTypeName, typeName);
    }

    [Fact]
    public void TestIntegrationEventTypeAttributeReturnsOverridenTypeName()
    {
        var typeName1 = IntegrationEventTypeAttribute.GetName(typeof(TestableIntegrationEvents.FakeHappened_1_0));
        var typeName2 = IntegrationEventTypeAttribute.GetName(typeof(TestableIntegrationEvents.FakeHappened_1_2));

        Assert.Equal(TestableIntegrationEvents.ThisHappenedTypeName, typeName1);
        Assert.Equal(TestableIntegrationEvents.ThatHappenedTypeName, typeName2);
        Assert.True(typeof(TestableIntegrationEvents.FakeHappened_1_0).IsAssignableFrom(typeof(TestableIntegrationEvents.FakeHappened_1_2)));
    }

    [Fact]
    public void TestIntegrationEventVersionAttributeReturnsVersionWhenTypeIsAttributed()
    {
        var version = IntegrationEventVersionAttribute.GetVersion(typeof(TestableIntegrationEvents.ThisHappened_1_1));

        Assert.Equal((1, 1), version);
    }

    [Fact]
    public void TestIntegrationEventVersionAttributeThrowsWhenTypeIsNotExplicitlyAttributed()
    {
        Assert.Throws<IntegrationEventException>(() => IntegrationEventVersionAttribute.GetVersion(typeof(TestableIntegrationEvents.FakeHappened_1_1)));        
    }
}

