using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.EntityAuditing.AzureTableStorage.Options;
using OneBeyond.Studio.EntityAuditing.Domain;
using OneBeyond.Studio.EntityAuditing.Infrastructure.DependencyInjection;

namespace OneBeyond.Studio.EntityAuditing.AzureTableStorage.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Specifies SQL Server as implementation for the Audit Manager
    /// </summary>
    /// <param name="entityAuditingBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IAuditingBuilder UseAzureTableStorage(
        this IAuditingBuilder entityAuditingBuilder,
        AzureTableStorageAuditingOptions options)
    {
        EnsureArg.IsNotNull(entityAuditingBuilder, nameof(entityAuditingBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        var services = entityAuditingBuilder.Services;

        services.AddSingleton(options);
        services.AddTransient(typeof(IAuditWriter<>), typeof(AuditTableStorageWriter<>));

        return entityAuditingBuilder;
    }
}
