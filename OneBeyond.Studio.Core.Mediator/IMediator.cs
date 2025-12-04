namespace OneBeyond.Studio.Core.Mediator;

public interface IMediator
{
    public Task<TResult> CommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>;

    public Task<TResult> QueryAsync<TQuery, TResult>(TQuery command, CancellationToken cancellationToken = default) where TQuery : ICommand<TResult>;

    public Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default);
}
