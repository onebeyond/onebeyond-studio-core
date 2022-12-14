using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.FileStorage.Domain;
using OneBeyond.Studio.FileStorage.FileSystem.Options;
using OneBeyond.Studio.FileStorage.Infrastructure.DependencyInjection;

namespace OneBeyond.Studio.FileStorage.FileSystem.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IFileStorageBuilder UseFileSystem(
        this IFileStorageBuilder fileRepositoryBuilder,
        FileSystemFileStorageOptions options)
    {
        EnsureArg.IsNotNull(fileRepositoryBuilder, nameof(fileRepositoryBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        fileRepositoryBuilder.Services.AddSingleton(options);

        fileRepositoryBuilder.Services.AddTransient<IFileStorage, FileSystemFileStorage>();

        return fileRepositoryBuilder;
    }
}
