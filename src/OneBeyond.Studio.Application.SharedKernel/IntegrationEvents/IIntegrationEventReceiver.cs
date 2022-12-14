using System;
using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

namespace OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

/// <summary>
/// </summary>
public interface IIntegrationEventReceiver
{
    /// <summary>
    /// </summary>
    /// <param name="processAsync"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunAsync(
        Func<IntegrationEvent, CancellationToken, Task> processAsync,
        CancellationToken cancellationToken);
}
