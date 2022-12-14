using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.EntityAuditing.Infrastructure.Initialisation;

public interface IAuditingInitialiser
{
    Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}
