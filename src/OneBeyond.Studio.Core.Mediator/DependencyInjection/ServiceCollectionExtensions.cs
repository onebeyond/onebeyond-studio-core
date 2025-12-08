using EnsureThat;
using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.Core.Mediator.DependencyInjection;
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core mediator used for OneBeyond.Studio.Core Mediator - will not work without this
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCoreMediator(this IServiceCollection services)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddScoped<IMediator, Mediator>();        

        return services;
    }
}
