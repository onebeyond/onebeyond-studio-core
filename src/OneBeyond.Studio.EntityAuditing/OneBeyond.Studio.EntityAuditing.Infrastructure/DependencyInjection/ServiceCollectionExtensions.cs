using System;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.EntityAuditing.Domain.AuditEntityNameResolvers;
using OneBeyond.Studio.EntityAuditing.Domain.AuditEventSerializers;
using OneBeyond.Studio.EntityAuditing.Infrastructure.Options;

namespace OneBeyond.Studio.EntityAuditing.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddEntityAuditing(
      this IServiceCollection services,
      IConfiguration configuration,
      EntityAuditingOptions options,
      Action<IAuditingBuilder> entityAuditingBuilderAction)
    {
        EnsureArg.IsNotNull(services, nameof(services));
        EnsureArg.IsNotNull(configuration, nameof(configuration));
        EnsureArg.IsNotNull(options, nameof(options));

        var builder = new AuditingBuilder(services, configuration);
        entityAuditingBuilderAction(builder);

        Audit.Core.Configuration.AuditDisabled = options.OmitChangeWriting;
        services.AddSingleton(options);
        services.AddTransient(typeof(IAuditNameResolver<>), typeof(AttributeNameResolver<>));
        services.AddTransient(typeof(IJsonAuditEventSerializer<>), typeof(JsonAuditEventSerializer<>));
    }
}
