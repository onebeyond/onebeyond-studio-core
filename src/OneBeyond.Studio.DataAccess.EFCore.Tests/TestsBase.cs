using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Repositories;
using Xunit;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

public abstract class TestsBase : IAsyncLifetime
{
    private IServiceScope? _serviceScope;

    protected IServiceProvider ServiceProvider { get; private set; } = default!;

    public ValueTask InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging();

        serviceCollection.AddSingleton<IConfiguration>(configuration);

        serviceCollection.AddScoped(typeof(IRORepository<,>), typeof(RORepository<,>));

        serviceCollection.AddScoped(typeof(IRWRepository<,>), typeof(RWRepository<,>));

        serviceCollection.AddScoped(typeof(IAggregateRootRWRepository<,,>), typeof(AggregateRootRWRepository<,,>));        

        ConfigureTestServices(configuration, serviceCollection);

        var containerBuilder = new ContainerBuilder();

        containerBuilder.Populate(serviceCollection);

        ConfigureTestServices(configuration, containerBuilder);

        var serviceProvider = new AutofacServiceProvider(containerBuilder.Build());

        _serviceScope = serviceProvider.CreateScope();

        ServiceProvider = _serviceScope.ServiceProvider;

        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

        LogManager.TryConfigure(loggerFactory);

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask DisposeAsync()
    {
        ServiceProvider = default!;
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
