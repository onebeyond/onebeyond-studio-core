using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Repositories;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

public abstract class TestsBase
{
    private IServiceScope? _serviceScope;

    protected IServiceProvider ServiceProvider { get; private set; } = default!;

    [TestInitialize]
    public void InitializeTest()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
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
    }

    [TestCleanup]
    public void CleanupTest()
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
