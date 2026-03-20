using Microsoft.Azure.SignalR.Management;

namespace OneBeyond.Studio.Infrastructure.SignalR;

public interface ISignalRService
{
    public Task PublishAsync<TMessageDto>(SignalRMessageDto<TMessageDto> message, CancellationToken cancellationToken);
    public Task PublishErrorMessageAsync(string message, string userId, CancellationToken cancellationToken);
    public Task<ServiceHubContext> GetHubContextAsync(CancellationToken cancellationToken);
}
