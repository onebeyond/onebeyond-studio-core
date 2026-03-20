using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Azure.SignalR.Management;

namespace OneBeyond.Studio.Infrastructure.SignalR;

public interface ISignalRService
{
    /// <summary>
    /// Publish a message to SignalR to a specific user
    /// </summary>
    /// <typeparam name="TMessageDto"></typeparam>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task PublishAsync<TMessageDto>(SignalRMessageDto<TMessageDto> message, CancellationToken cancellationToken);
    /// <summary>
    /// Publish an error message to a SignalR error channel to a specific user.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task PublishErrorMessageAsync(string message, Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Authenticate a frontend user to SignalR. Return the Url and AccessToken to the client.<br/>
    /// For normal operation this must be authenticated to function. For more complex strategies, make use of<br/>
    /// <see cref="GetHubContextAsync(CancellationToken)"/>
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="NegotiationResponse"/></returns>
    public Task<NegotiationResponse> NegotiateAsync(Guid userId, CancellationToken cancellationToken);
    /// <summary>
    /// Returns the Hub Context. Please use for further expansion of SignalR in more complex circumstances.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<ServiceHubContext> GetHubContextAsync(CancellationToken cancellationToken);
}
