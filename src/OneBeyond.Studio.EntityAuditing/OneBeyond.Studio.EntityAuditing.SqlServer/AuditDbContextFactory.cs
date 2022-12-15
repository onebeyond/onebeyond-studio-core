using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OneBeyond.Studio.EntityAuditing.SqlServer;

internal sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    AuditDbContext IDesignTimeDbContextFactory<AuditDbContext>.CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<AuditDbContext>();
        dbContextOptionsBuilder.UseSqlServer(
            configuration.GetConnectionString("ApplicationConnectionString"),
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", "audit"));
        var dbContextOptions = dbContextOptionsBuilder.Options;

        return new AuditDbContext(dbContextOptions);
    }
}
