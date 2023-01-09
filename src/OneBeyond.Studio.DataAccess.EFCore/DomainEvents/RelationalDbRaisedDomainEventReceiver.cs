using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.OwnedInstances;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Application.SharedKernel.DomainEvents;
using OneBeyond.Studio.Crosscuts.Activities;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Crosscuts.Logging;

namespace OneBeyond.Studio.DataAccess.EFCore.DomainEvents;

internal abstract class RelationalDbRaisedDomainEventReceiver<TDbContext> : IRaisedDomainEventReceiver
    where TDbContext : DbContext
{
    private const string ProcessReceivedDomainEventActivityName = "OneBeyond.Studio.DataAccess.DomainEventProcessing";

    private static readonly ILogger Logger = LogManager.CreateLogger<RelationalDbRaisedDomainEventReceiver<TDbContext>>();

    private readonly TDbContext _dbContext;

    protected RelationalDbRaisedDomainEventReceiver(Owned<TDbContext> dbContext)
    {
        EnsureArg.IsNotNull(dbContext, nameof(dbContext));

        _dbContext = dbContext.Value;
    }

    protected abstract string SelectForUpdateSql { get; }
    protected abstract string DeleteByIdSql { get; }

    public async Task RunAsync(
        Func<RaisedDomainEvent, CancellationToken, Task> processAsync,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(processAsync, nameof(processAsync));

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        while (true)
        {
            try
            {
                await strategy.ExecuteAsync(
                    async () =>
                    {
                        using (var transaction = await _dbContext.Database
                            .BeginTransactionAsync(cancellationToken)
                            .ConfigureAwait(false))
                        {
                            var raisedDomainEvent = await _dbContext.Set<RaisedDomainEvent>()
                                .FromSqlRaw(SelectForUpdateSql)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken)
                                .ConfigureAwait(false);
                            if (raisedDomainEvent is not null)
                            {
                                using (var activity = new Activity(ProcessReceivedDomainEventActivityName)
                                    .DoStart(raisedDomainEvent.ActivityId, raisedDomainEvent.ActivityTraceState))
                                {
                                    await processAsync(
                                            raisedDomainEvent.Clone(activity),
                                            cancellationToken)
                                        .ConfigureAwait(false);
                                }
                                await _dbContext.Database
                                    .ExecuteSqlRawAsync(DeleteByIdSql, raisedDomainEvent.Id)
                                    .ConfigureAwait(false);
                                await transaction.CommitAsync(CancellationToken.None).ConfigureAwait(false);
                            }
                            else
                            {
                                await transaction.DisposeAsync().ConfigureAwait(false);
                                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }).ConfigureAwait(false);
            }
            catch (OperationCanceledException operationCanceledException)
            when (!operationCanceledException.IsCritical())
            {
                Logger.LogInformation("Execution of domain event receiver has been cancelled");
                return;
            }
            catch (Exception exception)
            {
                Logger.LogError(
                    exception,
                    "Execution of domain event receiver has terminated due to error");
                throw;
            }
        }
    }
}
