using System;
using System.Reflection;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.DataAccess.EFCore.DomainEvents;
using OneBeyond.Studio.DataAccess.EFCore.Options;
using OneBeyond.Studio.DataAccess.EFCore.Projections;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.DataAccess.EFCore.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IDataAccessBuilder AddDataAccess<TDbContext>(
        this IServiceCollection services,
        DataAccessOptions options,
        Action<IServiceProvider, DbContextOptionsBuilder<TDbContext>> configureDbContext,
        Func<IServiceProvider, DbContextOptions<TDbContext>, bool, TDbContext> createDbContext)
        where TDbContext : DbContext
        => new DataAccessBuilder<TDbContext>(
            services,
            options,
            configureDbContext,
            createDbContext);

    public static IServiceCollection AddDbContextRaisedDomainEventReceiver<TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddTransient<IRaisedDomainEventReceiver, DbContextRaisedDomainEventReceiver<TDbContext>>();

        return services;
    }

    public static IServiceCollection AddMsSQLRaisedDomainEventReceiver<TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddTransient<IRaisedDomainEventReceiver, MsSQLRaisedDomainEventReceiver<TDbContext>>();

        return services;
    }

    public static IServiceCollection AddPostgreSQLRaisedDomainEventReceiver<TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddTransient<IRaisedDomainEventReceiver, PostgreSQLRaisedDomainEventReceiver<TDbContext>>();

        return services;
    }

    public static IServiceCollection AddEntityTypeProjections(
        this IServiceCollection services,
        params Assembly[] entityTypeProjectionsAssemblies)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.Scan((scan) =>
            scan.FromAssemblies(entityTypeProjectionsAssemblies)
                .AddClasses((classes) => classes.AssignableTo(typeof(IEntityTypeProjection<>)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime());

        return services;
    }
}
