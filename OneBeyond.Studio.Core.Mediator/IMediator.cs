using OneBeyond.Studio.Core.Mediator.Commands;
using OneBeyond.Studio.Core.Mediator.Notifications;
using OneBeyond.Studio.Core.Mediator.Queries;

namespace OneBeyond.Studio.Core.Mediator;

public interface IMediator
{
    public Task<TResult> CommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : class, ICommand<TResult>;

    public Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : class, IQuery<TResult>;

    public Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : class, INotification;
}
