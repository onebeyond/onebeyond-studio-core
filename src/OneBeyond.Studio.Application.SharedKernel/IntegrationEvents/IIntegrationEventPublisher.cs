using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

namespace OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

/// <summary>
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// </summary>
    /// <param name="integrationEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
