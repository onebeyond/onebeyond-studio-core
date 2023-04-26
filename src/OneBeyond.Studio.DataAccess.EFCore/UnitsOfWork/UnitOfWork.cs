using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using EnsureThat;
using Microsoft.Extensions.Options;
using OneBeyond.Studio.Application.SharedKernel.UnitsOfWork;

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
        DisposeUnit(); // This call is required for changes being applied right away.
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
        => DisposeUnit();

    private void DisposeUnit()
    {
        _transactionScope.Dispose();
        GC.SuppressFinalize(this);
    }
}
