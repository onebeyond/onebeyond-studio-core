namespace OneBeyond.Studio.Infrastructure.SignalR;

/// <summary>
/// SignalR Supports sending arbitrary Json, but must be sent to
/// a specific user, over a specific channel
/// </summary>
/// <typeparam name="TMessageDto">The structure of the arbitrary Json to be sent over SignalR</typeparam>
public class SignalRMessageDto<TMessageDto>
{
    /// <summary>
    /// The Message DTO
    /// </summary>
    public TMessageDto? Message { get; init; }
    /// <summary>
    /// The user to send the message to
    /// </summary>
    public required Guid UserId { get; init; }
    /// <summary>
    /// The channel to send the message to.
    /// </summary>
    public required string NotificationChannelName { get; init; }
}
