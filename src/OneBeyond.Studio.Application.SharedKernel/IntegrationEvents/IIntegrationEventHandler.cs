using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

namespace OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

/// <summary>
/// </summary>
/// <typeparam name="TIntegrationEvent"></typeparam>
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// </summary>
    /// <param name="integrationEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
