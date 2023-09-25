using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.DataAccess.EFCore.DependencyInjection;
using OneBeyond.Studio.DataAccess.EFCore.Options;
using OneBeyond.Studio.EntityAuditing.Domain;
using OneBeyond.Studio.EntityAuditing.Infrastructure.DependencyInjection;
using OneBeyond.Studio.EntityAuditing.Infrastructure.Initialisation;
using OneBeyond.Studio.EntityAuditing.SqlServer.DependencyInjection;
using OneBeyond.Studio.EntityAuditing.SqlServer.Initialisation;
using OneBeyond.Studio.EntityAuditing.SqlServer.Options;

namespace OneBeyond.Studio.EntityAuditing.SqlServer.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Specifies SQL Server as implementation for the Audit Manager
    /// </summary>
    /// <param name="entityAuditingBuilder"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IAuditingBuilder UseSqlServer(
        this IAuditingBuilder entityAuditingBuilder,
        SqlEntityAuditingOptions options)
    {
        EnsureArg.IsNotNull(entityAuditingBuilder, nameof(entityAuditingBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        var services = entityAuditingBuilder.Services;

        services.AddDataAccess<AuditDbContext>(
            new DataAccessOptions(),
            (serviceProvider, dbContextOptionsBuilder) =>
            {
                dbContextOptionsBuilder.UseSqlServer(
                    options.ConnectionString,
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "audit"));
            },
            (serviceProvider, dbContextOptions, areDomainEventsEnabled) => new AuditDbContext(dbContextOptions));


        services.AddScoped(typeof(IAuditEventRepository), typeof(AuditEventRepository));
        services.AddTransient(typeof(IAuditWriter<>), typeof(AuditDbWriter<>));

        services.AddSingleton(options);
        services.AddSingleton(typeof(IAuditingInitialiser), typeof(SqlEntityAuditingInitialiser));

        return entityAuditingBuilder;
    }
}
