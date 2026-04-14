using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Crosscuts.Logging;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Infrastructure;

public abstract class TestsBase : IDisposable
{
    private IServiceScope? _serviceScope;

    protected IServiceProvider ServiceProvider { get; private set; } = default!;

    protected void Init()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging();

        serviceCollection.AddSingleton<IConfiguration>(configuration);        

        ConfigureTestServices(configuration, serviceCollection);

        var containerBuilder = new ContainerBuilder();

        containerBuilder.Populate(serviceCollection);

        ConfigureTestServices(configuration, containerBuilder);

        var serviceProvider = new AutofacServiceProvider(containerBuilder.Build());

        _serviceScope = serviceProvider.CreateScope();

        ServiceProvider = _serviceScope.ServiceProvider;

        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

        LogManager.TryConfigure(loggerFactory);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            CleanupTest();
        }
    }

    private void CleanupTest()
    {
        ServiceProvider = default!;
        _serviceScope?.Dispose();
        _serviceScope = null;
    }

    protected abstract void ConfigureTestServices(
        IConfiguration configuration,
        IServiceCollection serviceCollection);

    protected abstract void ConfigureTestServices(
        IConfiguration configuration,
        ContainerBuilder containerBuilder);
}
