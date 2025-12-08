namespace OneBeyond.Studio.Core.Mediator.Notifications;

public interface INotificationHandler<in TNotification> where TNotification : INotification 
{ 
    /// <summary>
    /// Handles a notification. Does not handle pipeline behaviours. Can have multiple receivers.
    /// </summary>
    /// <param name="notification">Notification to send</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}

