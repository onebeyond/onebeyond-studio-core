using System;
using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.FileStorage.Domain.Options;

namespace OneBeyond.Studio.FileStorage.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the file manager using the given builder action
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="fileRecordsConnectionString">the connection string to the db where file records will be saved</param>
    /// <param name="fileStorageBuilderAction"></param>
    public static void AddFileStorage(
        this IServiceCollection services,
        Action<IFileStorageBuilder> fileStorageBuilderAction)
    {
        services.AddFileStorage(
            new MimeTypeValidationOptions
            {
                ValidationMode = MimeTypeValidationMode.Blacklist,
                MimeTypeSignatures = Array.Empty<MimeTypeSignatureOptions>()
            },
            fileStorageBuilderAction);
    }

    public static void AddFileStorage(
        this IServiceCollection services,
        MimeTypeValidationOptions validationOptions,
        Action<IFileStorageBuilder> fileStorageBuilderAction)
    {
        EnsureArg.IsNotNull(services, nameof(services));
        EnsureArg.IsNotNull(validationOptions, nameof(validationOptions));
        EnsureArg.IsNotNull(fileStorageBuilderAction, nameof(fileStorageBuilderAction));

        services.AddSingleton(validationOptions);

        var fileStorageBuilder = new FileStorageBuilder(services);

        fileStorageBuilderAction(fileStorageBuilder);
    }

    public static void AddCloudStorage(
        this IServiceCollection services,
        Action<ICloudStorageBuilder> cloudStorageBuilderAction)
    {
        EnsureArg.IsNotNull(services, nameof(services));
        EnsureArg.IsNotNull(cloudStorageBuilderAction, nameof(cloudStorageBuilderAction));

        var cloudStorageBuilder = new CloudStorageBuilder(services);
        cloudStorageBuilderAction(cloudStorageBuilder);
    }
}
