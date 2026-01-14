using OneBeyond.Studio.Core.Mediator.Notifications;

namespace OneBeyond.Studio.Core.Mediator;

public interface IMediator
{
    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) 
        where TRequest : class, IRequest;

    public Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);

    public Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) 
        where TNotification : class, INotification;
}
