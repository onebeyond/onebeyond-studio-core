using EnsureThat;
using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.FileStorage.Infrastructure.DependencyInjection;

internal sealed class FileStorageBuilder : IFileStorageBuilder
{
    public FileStorageBuilder(IServiceCollection services)
    {
        EnsureArg.IsNotNull(services, nameof(services));

        Services = services;
    }

    public IServiceCollection Services { get; }
}
