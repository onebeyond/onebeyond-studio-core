using Microsoft.EntityFrameworkCore;

namespace OneBeyond.Studio.DataAccess.EFCore.Transactions;

/// <summary>
/// ITransactionExecutor is for using with EnableRetryOnFailure only. It exists
/// so that you can still run transactions, while enabling Cloud Networking Infrastructure 
/// to work correctly.
/// <br/>
/// DO NOT USE ON EVERY DB REQUEST. This should only be used where your domain would be fundamentally
/// broken without the use of transactions. Otherwise you completely undo the benefits of EnableRetryOnFailure
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public interface ITransactionExecutor<TDbContext> where TDbContext : DbContext
{
    public Task ExecuteAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default);

    public Task<TResult> ExecuteAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
