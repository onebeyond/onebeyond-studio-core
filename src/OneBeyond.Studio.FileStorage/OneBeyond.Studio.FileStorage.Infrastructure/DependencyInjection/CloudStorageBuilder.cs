using EnsureThat;
using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.FileStorage.Infrastructure.DependencyInjection;

internal sealed class CloudStorageBuilder : ICloudStorageBuilder
{
    public CloudStorageBuilder(IServiceCollection services)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        Services = services;
    }

    public IServiceCollection Services { get; }
}
