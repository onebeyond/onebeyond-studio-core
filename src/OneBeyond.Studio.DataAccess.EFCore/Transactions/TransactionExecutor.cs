using Microsoft.EntityFrameworkCore;

namespace OneBeyond.Studio.DataAccess.EFCore.Transactions;

internal sealed class TransactionExecutor<TDbContext> : ITransactionExecutor<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext _domainContext;

    public TransactionExecutor(TDbContext domainContext)
    {
        ArgumentNullException.ThrowIfNull(domainContext);
        _domainContext = domainContext;
    }

    public async Task ExecuteAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var executionStrategy = _domainContext.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _domainContext.Database
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                await operation().ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                _domainContext.ChangeTracker.Clear();
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                throw;
            }
        }).ConfigureAwait(false);
    }

    public Task<TResult> ExecuteAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)        
    {
        ArgumentNullException.ThrowIfNull(operation);
        return ExecuteInternalAsync(operation, cancellationToken);
    }

    private async Task<TResult> ExecuteInternalAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken)        
    {
        var executionStrategy = _domainContext.Database.CreateExecutionStrategy();

        var result = await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _domainContext.Database
                        .BeginTransactionAsync(cancellationToken)
                        .ConfigureAwait(false);

            try
            {
                var result = await operation().ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                return result;
            }
            catch (Exception)
            {
                _domainContext.ChangeTracker.Clear();
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                throw;
            }
        }).ConfigureAwait(false);

        return result;
    }
}
