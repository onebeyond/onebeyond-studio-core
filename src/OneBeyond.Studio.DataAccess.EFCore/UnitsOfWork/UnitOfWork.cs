using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using EnsureThat;
using Microsoft.Extensions.Options;
using OneBeyond.Studio.Domain.SharedKernel.UnitsOfWork;

namespace OneBeyond.Studio.DataAccess.EFCore.UnitsOfWork;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly TransactionScope _transactionScope;

    public UnitOfWork(IOptions<UnitOfWorkOptions> options)
    {
        EnsureArg.IsNotNull(options, nameof(options));

        _transactionScope = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions
            {
                IsolationLevel = options.Value?.IsolationLevel
                    ?? IsolationLevel.ReadCommitted,
                Timeout = options.Value?.Timeout
                    ?? TransactionManager.DefaultTimeout,
            },
            TransactionScopeAsyncFlowOption.Enabled);
    }

    Task IUnitOfWork.CompleteAsync(CancellationToken cancellationToken)
    {
        _transactionScope.Complete();
        Dispose(true); // This call is required for changes being applied right away.
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
        => Dispose(true);

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _transactionScope.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
