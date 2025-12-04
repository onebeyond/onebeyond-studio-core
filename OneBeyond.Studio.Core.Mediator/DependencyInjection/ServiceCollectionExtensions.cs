using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.Core.Mediator.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddCoreMediator(this ServiceCollection services)
    {
        services.AddSingleton<IMediator, IMediator>();        

        return services;
    }
}
