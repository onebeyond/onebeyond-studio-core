using EnsureThat;
using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.Core.Mediator.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreMediator(this IServiceCollection services)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        services.AddScoped<IMediator, Mediator>();        

        return services;
    }
}
