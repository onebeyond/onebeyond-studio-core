using OneBeyond.Studio.Core.Mediator.Notifications;

namespace OneBeyond.Studio.Core.Mediator;

public interface IMediator
{
    /// <summary>
    /// Send a request (a command or a query) to the domain - not expecting a return value.
    /// </summary>
    /// <typeparam name="TRequest">Type of request</typeparam>
    /// <param name="request">Request to send</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) 
        where TRequest : class, IRequest;

    /// <summary>
    /// Send a request (a command or a query) to the domain - not expecting a return value.
    /// </summary>
    /// <param name="request">Request to send</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task Send(IRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a request (a command or a query) to the domain - expecting a response of type <see cref="TResult"/>
    /// </summary>
    /// <typeparam name="TResult">Type of result</typeparam>
    /// <param name="request">Request to send</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notify all listeners of <see cref="TNotification"/>. Does not apply pipeline behaviours.
    /// </summary>
    /// <typeparam name="TNotification">Type of notification for listeners</typeparam>
    /// <param name="notification">Notification</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) 
        where TNotification : class, INotification;
}
