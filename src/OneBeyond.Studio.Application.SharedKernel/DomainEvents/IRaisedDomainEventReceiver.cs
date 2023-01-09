using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Application.SharedKernel.DomainEvents;

/// <summary>
/// </summary>
public interface IRaisedDomainEventReceiver
{
    /// <summary>
    /// </summary>
    /// <param name="processAsync"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunAsync(
        Func<RaisedDomainEvent, CancellationToken, Task> processAsync,
        CancellationToken cancellationToken);
}
