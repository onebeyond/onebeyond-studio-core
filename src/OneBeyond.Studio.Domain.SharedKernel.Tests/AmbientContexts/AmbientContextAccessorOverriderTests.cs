using System;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OneBeyond.Studio.Domain.SharedKernel.AmbientContexts;
using OneBeyond.Studio.Domain.SharedKernel.DependencyInjection;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.AmbientContexts;

[TestClass]
public sealed class AmbientContextAccessorOverriderTests
{
    [TestMethod]
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
        Assert.IsInstanceOfType(ambientContextAccessor, typeof(AmbientContextAccessorOverrider<TestableAmbientContext>));
        Assert.AreEqual("42", ambientContext.StringValue);
    }

    [TestMethod]
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
        Assert.AreEqual("outer", outerAmbientContext1.StringValue);
        Assert.AreEqual("outer", outerAmbientContext2.StringValue);
        Assert.AreEqual("inner", innerAmbientContext1.StringValue);
    }

    [TestMethod]
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
        Assert.IsNotNull(invalidOperationException, nameof(invalidOperationException));
        Assert.AreEqual("Ambient context accessor overriding order is broken.", invalidOperationException?.Message);
        Assert.AreEqual("42", globalAmbientContext.StringValue);
    }
}
