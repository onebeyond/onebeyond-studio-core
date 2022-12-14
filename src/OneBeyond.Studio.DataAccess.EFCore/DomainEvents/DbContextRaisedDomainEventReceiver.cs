using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.OwnedInstances;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.DataAccess.EFCore.DomainEvents;

internal sealed class DbContextRaisedDomainEventReceiver<TDbContext> : IRaisedDomainEventReceiver
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly DbSet<RaisedDomainEvent> _raisedDomainEvents;

    public DbContextRaisedDomainEventReceiver(Owned<TDbContext> dbContext)
    {
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));

        _dbContext = dbContext.Value;
        _raisedDomainEvents = _dbContext.Set<RaisedDomainEvent>();
    }

    public async Task RunAsync(
        Func<RaisedDomainEvent, CancellationToken, Task> processAsync,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(processAsync, nameof(processAsync));

        while (true)
        {
            try
            {
                var raisedDomainEvent = await _raisedDomainEvents
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (raisedDomainEvent is not null)
                {
                    await processAsync(raisedDomainEvent, cancellationToken).ConfigureAwait(false);
                    _raisedDomainEvents.Remove(raisedDomainEvent);
                    await _dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException operationCanceledException)
            when (!operationCanceledException.IsCritical())
            {
                return;
            }
        }
    }
}
