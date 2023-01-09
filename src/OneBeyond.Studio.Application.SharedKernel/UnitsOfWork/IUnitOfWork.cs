using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Application.SharedKernel.UnitsOfWork;

/// <summary>
/// Represents the Unit of Work pattern.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Completes unit of work.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
