using Autofac;
using Moq;
using OneBeyond.Studio.Application.SharedKernel.AmbientContexts;
using OneBeyond.Studio.Application.SharedKernel.DependencyInjection;
using Xunit;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.AmbientContexts;


public sealed class AmbientContextAccessorOverriderTests
{
    [Fact]
    public void Returns_original_context_when_no_override_provided()
    {
        // Arrange
        var containerBuilder = new ContainerBuilder();
        containerBuilder.AddAmbientContextAccessor<TestableAmbientContextAccessor, TestableAmbientContext>(withOverrider: true);
        var container = containerBuilder.Build();

        // Act
        var ambientContextAccessor = container.Resolve<IAmbientContextAccessor<TestableAmbientContext>>();
        var ambientContext = ambientContextAccessor.AmbientContext;

        // Assert
        Assert.IsType<AmbientContextAccessorOverrider<TestableAmbientContext>>(ambientContextAccessor);
        Assert.Equal("42", ambientContext.StringValue);
    }

    [Fact]
    public void Returns_context_based_on_override_scope()
    {
        // Arrange
        var containerBuilder = new ContainerBuilder();
        containerBuilder.AddAmbientContextAccessor<TestableAmbientContextAccessor, TestableAmbientContext>(withOverrider: true);
        var container = containerBuilder.Build();
        var outerAmbientContextAccessorMock = new Mock<IAmbientContextAccessor<TestableAmbientContext>>();
        outerAmbientContextAccessorMock
            .Setup((ambientContextAccessor) => ambientContextAccessor.AmbientContext)
            .Returns(new TestableAmbientContext("outer"));
        var outerAmbientContextAccessor = outerAmbientContextAccessorMock.Object;
        var innerAmbientContextAccessorMock = new Mock<IAmbientContextAccessor<TestableAmbientContext>>();
        innerAmbientContextAccessorMock
            .Setup((ambientContextAccessor) => ambientContextAccessor.AmbientContext)
            .Returns(new TestableAmbientContext("inner"));
        var innerAmbientContextAccessor = innerAmbientContextAccessorMock.Object;
        var ambientContextAccessor = container.Resolve<IAmbientContextAccessor<TestableAmbientContext>>();

        // Act
        var outerAmbientContext1 = default(TestableAmbientContext);
        var outerAmbientContext2 = default(TestableAmbientContext);
        var innerAmbientContext1 = default(TestableAmbientContext);
        using (AmbientContextAccessorOverrider<TestableAmbientContext>.OverrideWith(outerAmbientContextAccessor))
        {
            outerAmbientContext1 = ambientContextAccessor.AmbientContext;

            using (AmbientContextAccessorOverrider<TestableAmbientContext>.OverrideWith(innerAmbientContextAccessor))
            {
                innerAmbientContext1 = ambientContextAccessor.AmbientContext;
            }

            outerAmbientContext2 = ambientContextAccessor.AmbientContext;
        }

        // Assert
        Assert.Equal("outer", outerAmbientContext1.StringValue);
        Assert.Equal("outer", outerAmbientContext2.StringValue);
        Assert.Equal("inner", innerAmbientContext1.StringValue);
    }

    [Fact]
    public void Throws_when_override_scope_disposing_is_broken()
    {
        // Arrange
        var containerBuilder = new ContainerBuilder();
        containerBuilder.AddAmbientContextAccessor<TestableAmbientContextAccessor, TestableAmbientContext>(withOverrider: true);
        var container = containerBuilder.Build();
        var outerAmbientContextAccessorMock = new Mock<IAmbientContextAccessor<TestableAmbientContext>>();
        outerAmbientContextAccessorMock
            .Setup((ambientContextAccessor) => ambientContextAccessor.AmbientContext)
            .Returns(new TestableAmbientContext("outer"));
        var outerAmbientContextAccessor = outerAmbientContextAccessorMock.Object;
        var innerAmbientContextAccessorMock = new Mock<IAmbientContextAccessor<TestableAmbientContext>>();
        innerAmbientContextAccessorMock
            .Setup((ambientContextAccessor) => ambientContextAccessor.AmbientContext)
            .Returns(new TestableAmbientContext("inner"));
        var innerAmbientContextAccessor = innerAmbientContextAccessorMock.Object;
        var ambientContextAccessor = container.Resolve<IAmbientContextAccessor<TestableAmbientContext>>();

        // Act
        var globalAmbientContext = ambientContextAccessor.AmbientContext;
        var invalidOperationException = default(InvalidOperationException);
        using (var outerOverride = AmbientContextAccessorOverrider<TestableAmbientContext>.OverrideWith(outerAmbientContextAccessor))
        using (var innerOverride = AmbientContextAccessorOverrider<TestableAmbientContext>.OverrideWith(innerAmbientContextAccessor))
        {
            try
            {
                outerOverride.Dispose();
            }
            catch (InvalidOperationException exception)
            {
                invalidOperationException = exception;
            }
        }

        // Assert
        Assert.NotNull(invalidOperationException);
        Assert.Equal("Ambient context accessor overriding order is broken.", invalidOperationException?.Message);
        Assert.Equal("42", globalAmbientContext.StringValue);
    }
}


