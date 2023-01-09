using EnsureThat;
using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.Hosting.AspNet.ModelBinders.MixedSource.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMixedSourceBinder(this IServiceCollection serviceCollection)
    {
        EnsureArg.IsNotNull(serviceCollection, nameof(serviceCollection));

        serviceCollection.ConfigureOptions<MixedModelBinderSetup>();

        return serviceCollection;
    }
}
