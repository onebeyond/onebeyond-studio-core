using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OneBeyond.Studio.Infrastructure.Azure.Tests;

public abstract class TestsBase : IAsyncLifetime
{
    private IServiceScope? _serviceScope;

    protected IServiceProvider? ServiceProvider { get; private set; }

    public ValueTask InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);

        ConfigureTestServices(configuration, serviceCollection);

        var containerBuilder = new ContainerBuilder();

        containerBuilder.Populate(serviceCollection);

        ConfigureTestServices(configuration, containerBuilder);

        var serviceProvider = new AutofacServiceProvider(containerBuilder.Build());

        _serviceScope = serviceProvider.CreateScope();

        ServiceProvider = _serviceScope.ServiceProvider;

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask DisposeAsync()
    {
        ServiceProvider = null;
        _serviceScope?.Dispose();
        _serviceScope = null;

        return ValueTask.CompletedTask;
    }

    protected abstract void ConfigureTestServices(
        IConfiguration configuration,
        IServiceCollection serviceCollection);

    protected abstract void ConfigureTestServices(
        IConfiguration configuration,
        ContainerBuilder containerBuilder);
}
