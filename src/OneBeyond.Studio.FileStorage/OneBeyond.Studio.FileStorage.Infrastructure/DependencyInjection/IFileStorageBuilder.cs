using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.FileStorage.Infrastructure.DependencyInjection;

public interface IFileStorageBuilder
{
    IServiceCollection Services { get; }
}
