using System;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.DependencyInjection;

namespace OneBeyond.Studio.EntityAuditing.Infrastructure.Initialisation;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Initialise the entity auditing manager (migrating database if needed)
    /// </summary>
    /// <param name="serviceProvider"></param>
    public static Task InitialiseEntityAuditingAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

        var auditManagerInitialiser = serviceProvider.GetService<IAuditingInitialiser>();

        return auditManagerInitialiser == null
            ? Task.CompletedTask
            : auditManagerInitialiser.InitializeAsync(serviceProvider, cancellationToken);
    }
}
