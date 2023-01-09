using Autofac;
using Autofac.Extensions.DependencyInjection;
using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Hosting.BackgroundServices;
using OneBeyond.Studio.Hosting.DomainEvents;
using OneBeyond.Studio.Hosting.IntegrationEvents;

namespace OneBeyond.Studio.Hosting.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainEventRelayAsHostedService(this IServiceCollection services)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddHostedService<DomainEventRelay>();

        return services;
    }

    public static IServiceCollection AddDomainEventRelayAsBackgroundService(
        this IServiceCollection services,
        Action<ContainerBuilder>? configureServices = default)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddBackgroundService<DomainEventRelay>(configureServices);

        return services;
    }

    public static IServiceCollection AddDomainEventRelayAsBackgroundService(
        this IServiceCollection services,
        Action<IServiceCollection> configureServices)
    {
        EnsureArg.IsNotNull(services, nameof(services));
        EnsureArg.IsNotNull(configureServices, nameof(configureServices));

        services.AddBackgroundService<DomainEventRelay>(configureServices);

        return services;
    }


    public static IServiceCollection AddMQPublishingDomainEventRelayAsBackgroundService(
        this IServiceCollection services,
        Action<ContainerBuilder>? configureServices = default)
    {
        return services.AddMQPublishingDomainEventRelayAsBackgroundService<DefaultMQRelay>(configureServices);
    }

    public static IServiceCollection AddMQPublishingDomainEventRelayAsBackgroundService(
        this IServiceCollection services,
        Action<IServiceCollection> configureServices)
    {
        return services.AddMQPublishingDomainEventRelayAsBackgroundService<DefaultMQRelay>(configureServices);
    }

    public static IServiceCollection AddMQReceivingDomainEventRelayAsBackgroundService(
        this IServiceCollection services,
        Action<ContainerBuilder>? configureServices = default)
    {
        return services.AddMQReceivingDomainEventRelayAsBackgroundService<DefaultMQRelay>(configureServices);
    }

    public static IServiceCollection AddMQReceivingDomainEventRelayAsBackgroundService(
        this IServiceCollection services,
        Action<IServiceCollection> configureServices)
    {
        return services.AddMQReceivingDomainEventRelayAsBackgroundService<DefaultMQRelay>(configureServices);
    }

    public static IServiceCollection AddMQPublishingDomainEventRelayAsBackgroundService<TRelayDiscriminator>(
        this IServiceCollection services,
        Action<ContainerBuilder>? configureServices = default)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddBackgroundService<MQPublishingDomainEventRelay<TRelayDiscriminator>>(configureServices);

        return services;
    }

    public static IServiceCollection AddMQPublishingDomainEventRelayAsBackgroundService<TRelayDiscriminator>(
        this IServiceCollection services,
        Action<IServiceCollection> configureServices)
    {
        EnsureArg.IsNotNull(services, nameof(services));
        EnsureArg.IsNotNull(configureServices, nameof(configureServices));

        services.AddBackgroundService<MQPublishingDomainEventRelay<TRelayDiscriminator>>(configureServices);

        return services;
    }

    public static IServiceCollection AddMQReceivingDomainEventRelayAsBackgroundService<TRelayDiscriminator>(
        this IServiceCollection services,
        Action<ContainerBuilder>? configureServices = default)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddBackgroundService<MQReceivingDomainEventRelay<TRelayDiscriminator>>(configureServices);

        return services;
    }

    public static IServiceCollection AddMQReceivingDomainEventRelayAsBackgroundService<TRelayDiscriminator>(
        this IServiceCollection services,
        Action<IServiceCollection> configureServices)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddBackgroundService<MQReceivingDomainEventRelay<TRelayDiscriminator>>(configureServices);

        return services;
    }

    public static IServiceCollection AddIntegrationEventRelayAsHostedService(this IServiceCollection services)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddHostedService<IntegrationEventRelay>();

        return services;
    }

    public static IServiceCollection AddIntegrationEventRelayAsBackgroundService(
        this IServiceCollection services,
        Action<ContainerBuilder>? configureServices = default)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddBackgroundService<IntegrationEventRelay>(configureServices);

        return services;
    }

    public static IServiceCollection AddIntegrationEventRelayAsBackgroundService(
        this IServiceCollection services,
        Action<IServiceCollection> configureServices)
    {
        EnsureArg.IsNotNull(services, nameof(services));
        EnsureArg.IsNotNull(configureServices, nameof(configureServices));

        services.AddBackgroundService<IntegrationEventRelay>(configureServices);

        return services;
    }

    public static IServiceCollection AddBackgroundService<TBackgroundService>(
        this IServiceCollection services,
        Action<IServiceCollection>? configureServices)
        where TBackgroundService : class, IBackgroundService
    {
        EnsureArg.IsNotNull(services, nameof(services));

        return services.AddBackgroundService<TBackgroundService>(
            configureServices is null
                ? default
                : (containerBuilder) =>
                  {
                      var serviceCollection = new ServiceCollection();
                      configureServices(serviceCollection);
                      containerBuilder.Populate(serviceCollection);
                  });
    }

    public static IServiceCollection AddBackgroundService<TBackgroundService>(
        this IServiceCollection services,
        Action<ContainerBuilder>? configureServices)
        where TBackgroundService : class, IBackgroundService
    {
        EnsureArg.IsNotNull(services, nameof(services));

        var backgroundServiceHostOptions =
            new BackgroundServiceHostOptions<TBackgroundService>
            {
                ConfigureServices = configureServices
            };

        services.AddSingleton(backgroundServiceHostOptions);

        services.AddScoped<TBackgroundService>();

        services.AddHostedService<BackgroundServiceHost<TBackgroundService>>();

        return services;
    }
    private class DefaultMQRelay
    {
    }
}
