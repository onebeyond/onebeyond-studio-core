using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.EntityAuditing.Infrastructure.DependencyInjection;

internal sealed class AuditingBuilder : IAuditingBuilder
{
    public AuditingBuilder(
        IServiceCollection services,
        IConfiguration configuration)
    {
        EnsureArg.IsNotNull(services, nameof(services));
        EnsureArg.IsNotNull(configuration, nameof(configuration));

        Services = services;
        Configuration = configuration;
    }

    public IServiceCollection Services { get; }

    public IConfiguration Configuration { get; }
}
