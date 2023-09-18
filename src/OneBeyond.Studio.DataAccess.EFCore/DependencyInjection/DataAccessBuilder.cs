using System;
using System.Transactions;
using Castle.DynamicProxy;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.DataAccess.EFCore.DomainEvents;
using OneBeyond.Studio.DataAccess.EFCore.Options;
using OneBeyond.Studio.DataAccess.EFCore.Projections;
using OneBeyond.Studio.DataAccess.EFCore.RelationalTypeMappings;
using OneBeyond.Studio.DataAccess.EFCore.UnitsOfWork;
using OneBeyond.Studio.Application.SharedKernel.AmbientContexts;
using OneBeyond.Studio.Application.SharedKernel.UnitsOfWork;
using Thinktecture;
using Z.EntityFramework.Extensions;
using OneBeyond.Studio.Application.SharedKernel.DomainEvents;
using OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;
using System.Collections.Generic;

namespace OneBeyond.Studio.DataAccess.EFCore.DependencyInjection;

internal abstract class DataAccessBuilder : IDataAccessBuilder
{
    protected DataAccessBuilder(IServiceCollection services)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        EntityFrameworkManager.IsCommunity = true; //To raise an exception in case if any paid features of Z.EntityFramework.Extensions library are used.
        AreDomainEventsEnabled = false;
        Services = services;
    }

    protected static ProxyGenerator ProxyGenerator { get; } = new();
    protected bool AreDomainEventsEnabled { get; private set; }
    protected bool AreIntegrationEventsEnabled { get; private set; }
    protected IServiceCollection Services { get; }

    public IDataAccessBuilder WithDomainEvents()
    {
        AreDomainEventsEnabled = true;
        Services.AddTransient<IPreSaveDomainEventDispatcher, DIBasedPreSaveDomainEventDispatcher>();
        Services.AddTransient<IPostSaveDomainEventDispatcher, DIBasedPostSaveDomainEventDispatcher>();
        return this;
    }

    public IDataAccessBuilder WithIntegrationEvents<TIntegrationEventDispatcher>()
        where TIntegrationEventDispatcher : class, IIntegrationEventDispatcher
    {
        AreIntegrationEventsEnabled = true;
        Services.AddTransient<IIntegrationEventDispatcher, TIntegrationEventDispatcher>();
        return WithDomainEvents();
    }

    public IDataAccessBuilder WithUnitOfWork(TimeSpan? timeout = default, IsolationLevel? isolationLevel = default)
    {
        Services.Configure<UnitOfWorkOptions>(
            (unitOfWorkOptions) =>
            {
                unitOfWorkOptions.Timeout = timeout;
                unitOfWorkOptions.IsolationLevel = isolationLevel;
            });
        Services.AddScoped<IUnitOfWork, UnitOfWork>();
        return this;
    }
}

internal sealed class DataAccessBuilder<TDbContext> : DataAccessBuilder
    where TDbContext : DbContext
{
    public DataAccessBuilder(
        IServiceCollection services,
        DataAccessOptions options,
        Action<IServiceProvider, DbContextOptionsBuilder<TDbContext>> configureDbContext,
        Func<IServiceProvider, DbContextOptions<TDbContext>, bool, bool, TDbContext> createDbContext)
        : base(services)
    {
        EnsureArg.IsNotNull(options, nameof(options));
        EnsureArg.IsNotNull(configureDbContext, nameof(configureDbContext));
        EnsureArg.IsNotNull(createDbContext, nameof(createDbContext));

        Services.AddSingleton(options);

        Services.AddScoped(
            (serviceProvider) =>
            {
                var options = serviceProvider.GetRequiredService<DataAccessOptions>();
                var dbContextOptionsBuilder = new DbContextOptionsBuilder<TDbContext>();
                dbContextOptionsBuilder.UseApplicationServiceProvider(serviceProvider); // Logging won't be configured for EF if it is not used. Maybe, it deprecates the registrations commented above as well.
                dbContextOptionsBuilder.EnableDetailedErrors(options.EnableDetailedErrors);
                dbContextOptionsBuilder.EnableSensitiveDataLogging(options.EnableSensitiveDataLogging);
                configureDbContext(serviceProvider, dbContextOptionsBuilder);
                dbContextOptionsBuilder.AddRelationalTypeMappingSourcePlugin<SmartEnumTypeMappingSourcePlugin>();
                var dbContextOptions = dbContextOptionsBuilder.Options;
                var dbContext = createDbContext(serviceProvider, dbContextOptions, AreDomainEventsEnabled, AreIntegrationEventsEnabled);
                if (AreDomainEventsEnabled)
                {
                    var preSaveDomainEventDispatcher = serviceProvider.GetRequiredService<IPreSaveDomainEventDispatcher>();
                    var ambientContextAccessors = serviceProvider.GetServices<IAmbientContextAccessor>();
                    var domainEventsProcessor = new DomainEventsProcessor(preSaveDomainEventDispatcher, ambientContextAccessors);
                    var processors = new List<IInterceptor> { domainEventsProcessor };

                    if (AreIntegrationEventsEnabled)
                    {
                        var integrationEventDispatcher = serviceProvider.GetRequiredService<IIntegrationEventDispatcher>();
                        var integrationEventsProcessor = new IntegrationEventsProcessor(integrationEventDispatcher);
                        processors.Add(integrationEventsProcessor);
                    }

                    dbContext = (TDbContext)ProxyGenerator.CreateClassProxyWithTarget(
                        typeof(TDbContext),
                        new[] { typeof(IInfrastructure<IServiceProvider>) }, // Potentially it requires intercepting all interfaces implemented by DbContext
                        dbContext,
                        processors.ToArray());
                }

                return dbContext;
            });

        Services.AddSingleton(typeof(IEntityTypeProjections<>), typeof(EntityTypeProjections<>));
    }
}
