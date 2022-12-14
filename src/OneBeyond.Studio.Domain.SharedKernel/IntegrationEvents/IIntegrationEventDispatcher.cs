using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

/// <summary>
/// </summary>
public interface IIntegrationEventDispatcher
{
    /// <summary>
    /// </summary>
    /// <param name="integrationEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DispatchAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
