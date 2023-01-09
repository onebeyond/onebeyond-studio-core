using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.EntityAuditing.Infrastructure.Initialisation;

namespace OneBeyond.Studio.EntityAuditing.SqlServer.Initialisation;

internal sealed class SqlEntityAuditingInitialiser : IAuditingInitialiser
{
    public async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using (var dbContext = serviceProvider.GetRequiredService<AuditDbContext>())
        {
            await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
