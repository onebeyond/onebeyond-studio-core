using Xunit;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Entities;

public sealed class ValueObjects : IClassFixture<LogManagerFixture>
{
    [Fact]
    public void TestValueObjectsOfTheSameTypeAreEqualBasedTheirOnPropertyValues()
    {
        var valueObject1 = new SomeValueObject(1, 2, 3);
        var valueObject2 = new SomeValueObject(1, 2, 3);

        Assert.True(valueObject1 == valueObject2);

        Assert.False(valueObject1 != valueObject2);

        Assert.Equal(valueObject1, valueObject2);
    }

    [Fact]
    public void TestValueObjectOfOneTypeAndValueObjectOfDerivedTypeAreAlwaysDifferent()
    {
        var valueObject1 = new SomeValueObject(1, 2, 3);
        var valueObject2 = (SomeValueObject)new DerivedValueObject(1, 2, 3);

        Assert.False(valueObject1 == valueObject2);

        Assert.True(valueObject1 != valueObject2);

        Assert.NotEqual(valueObject1, valueObject2);
    }
}
