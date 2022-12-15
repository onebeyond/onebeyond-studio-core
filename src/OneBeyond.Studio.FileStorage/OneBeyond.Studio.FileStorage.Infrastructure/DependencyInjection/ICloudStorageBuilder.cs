using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.FileStorage.Infrastructure.DependencyInjection;

public interface ICloudStorageBuilder
{
    IServiceCollection Services { get; }
}
