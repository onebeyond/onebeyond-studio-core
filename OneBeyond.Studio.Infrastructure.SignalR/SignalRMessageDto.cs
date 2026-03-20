namespace OneBeyond.Studio.Infrastructure.SignalR;

public class SignalRMessageDto<TMessageDto>
{
    public TMessageDto? Message { get; init; }
    public required string UserId { get; init; }
    public required string NotificationChannelName { get; init; }
}
