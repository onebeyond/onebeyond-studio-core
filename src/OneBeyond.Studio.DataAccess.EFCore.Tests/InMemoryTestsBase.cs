using System;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.DataAccess.EFCore.DependencyInjection;
using OneBeyond.Studio.DataAccess.EFCore.Options;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

public abstract class InMemoryTestsBase : TestsBase
{
    private readonly bool _withDomainEvents;

    protected InMemoryTestsBase(bool withDomainEvents)
    {
        _withDomainEvents = withDomainEvents;
    }

    protected override void ConfigureTestServices(
        IConfiguration configuration,
        IServiceCollection serviceCollection)
    {
        var databaseName = Guid.NewGuid().ToString();

        var infrastructureBuilder = serviceCollection.AddDataAccess<DbContexts.DbContext>(
            new DataAccessOptions(),
            (serviceProvider, dbContextOptionsBuilder) =>
            {
                dbContextOptionsBuilder
                    .ConfigureWarnings((warnings) => warnings.Throw())
                    .UseInMemoryDatabase(databaseName)
                    .ReplaceService<IModelCacheKeyFactory, ModelCacheKeyFactory>();
            },
            (serviceProvider, dbContextOptions, areDomainEventsEnabled, areIntegrationEventsEnabled) =>
                new DbContexts.DbContext(dbContextOptions, areDomainEventsEnabled, areIntegrationEventsEnabled));

        if (_withDomainEvents)
        {
            infrastructureBuilder.WithDomainEvents();
            serviceCollection.AddDbContextRaisedDomainEventReceiver<DbContexts.DbContext>();
        }
    }

    protected override void ConfigureTestServices(
        IConfiguration configuration,
        ContainerBuilder containerBuilder)
    {
    }

    private sealed class ModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(Microsoft.EntityFrameworkCore.DbContext context, bool designTime)
            => context is DbContexts.DbContext dbContext
                ? (context.GetType(), dbContext.AreDomainEventsEnabled)
                : (object)context.GetType();
    }
}
