using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OneBeyond.Studio.Hosting.BackgroundServices;
using OneBeyond.Studio.Hosting.DependencyInjection;

namespace OneBeyond.Studio.Hosting.Tests.BackgroundServices;

[TestClass]
public sealed class BackgroundServiceHostTests
{
    [TestMethod]
    public async Task Uses_globally_registered_services()
    {
        // Arrange
        var backgroundServiceDependencyMock = new Mock<ITestableBackgroundServiceDependency>();
        var backgroundServiceDependency = backgroundServiceDependencyMock.Object;
        var backgroundServiceHost = ConfigureBackgroundServiceHost(backgroundServiceDependency, localDependency: null);

        // Act
        await backgroundServiceHost.StartAsync(CancellationToken.None);

        // Technically this test contains a race condition, because StartAsync does not await the execution of the service
        // because that would defeat the purpose of the host.
        await Task.Delay(2000);

        // Assert
        backgroundServiceDependencyMock.Verify((dependency) => dependency.DoSomething(), Times.Once);
    }

    [TestMethod]
    public async Task Uses_locally_registered_service()
    {
        // Arrange
        var globalBackgroundServiceDependencyMock = new Mock<ITestableBackgroundServiceDependency>();
        var globalBackgroundServiceDependency = globalBackgroundServiceDependencyMock.Object;
        var localBackgroundServiceDependencyMock = new Mock<ITestableBackgroundServiceDependency>();
        var localBackgroundServiceDependency = localBackgroundServiceDependencyMock.Object;
        var backgroundServiceHost = ConfigureBackgroundServiceHost(
            globalBackgroundServiceDependency,
            localBackgroundServiceDependency);

        // Act
        await backgroundServiceHost.StartAsync(CancellationToken.None);
        // Technically this test contains a race condition, because StartAsync does not await the execution of the service
        // because that would defeat the purpose of the host.
        await Task.Delay(2000);

        // Assert
        globalBackgroundServiceDependencyMock.Verify((dependency) => dependency.DoSomething(), Times.Never);
        localBackgroundServiceDependencyMock.Verify((dependency) => dependency.DoSomething(), Times.Once);
    }

    private static IHostedService ConfigureBackgroundServiceHost(
        ITestableBackgroundServiceDependency globalDependency,
        ITestableBackgroundServiceDependency? localDependency)
    {
        var services = new ServiceCollection();        
        services.AddSingleton(globalDependency);
        services.AddBackgroundService<TestableBackgroundService>(
            localDependency is null
                ? null
                : new Action<IServiceCollection>(
                    (services) =>
                    {
                        services.AddSingleton(localDependency);
                    }));
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(services);
        var container = containerBuilder.Build();
        return container.Resolve<IEnumerable<IHostedService>>()
            .Single((service) => service is BackgroundServiceHost<TestableBackgroundService>);
    }
}
