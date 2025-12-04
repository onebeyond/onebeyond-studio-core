namespace OneBeyond.Studio.Core.Mediator;

public interface INotificationHandler<in TNotification> where TNotification : INotification { 
    public Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}

