using System;
using System.Transactions;

namespace OneBeyond.Studio.DataAccess.EFCore.UnitsOfWork;

/// <summary>
/// Specifies options for configuring unit of work.
/// </summary>
internal sealed class UnitOfWorkOptions
{
    /// <summary>
    /// Timeout to be used by unit of work. Default value means <see cref="TransactionManager.DefaultTimeout"/>.
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    /// <summary>
    /// Isolation level to be used by unit of work. Default values means <see cref="IsolationLevel.ReadCommitted"/>.
    /// </summary>
    public IsolationLevel? IsolationLevel { get; set; }
}
