using OneBeyond.Studio.Core.Mediator.Commands;
using OneBeyond.Studio.Core.Mediator.Notifications;
using OneBeyond.Studio.Core.Mediator.Queries;

namespace OneBeyond.Studio.Core.Mediator;

public interface IMediator
{
    /// <summary>
    /// Send a command to the domain - not expecting a return value.
    /// </summary>
    /// <typeparam name="TCommand">Type of Command</typeparam>
    /// <param name="command">Command to send</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task CommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : class, ICommand;

    /// <summary>
    /// Send a command to the domain - expecting a response of type <see cref="TResult"/>
    /// </summary>
    /// <typeparam name="TCommand">Type of Command</typeparam>
    /// <typeparam name="TResult">Type of result</typeparam>
    /// <param name="command">Command to send</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<TResult> CommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : class, ICommand<TResult>;

    /// <summary>
    /// Query the domain. Expecing a respone of type <see cref="TResult"/>
    /// </summary>
    /// <typeparam name="TQuery">Type of Query</typeparam>
    /// <typeparam name="TResult">Type of result</typeparam>
    /// <param name="query">Query to send</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : class, IQuery<TResult>;

    /// <summary>
    /// Notify all listeners of <see cref="TNotification"/>. Does not apply pipeline behaviours.
    /// </summary>
    /// <typeparam name="TNotification">Type of notification for listeners</typeparam>
    /// <param name="notification">Notification</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : class, INotification;
}
