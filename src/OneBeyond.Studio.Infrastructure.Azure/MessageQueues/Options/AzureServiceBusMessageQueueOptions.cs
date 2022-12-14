namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

public record AzureServiceBusMessageQueueOptions : AzureMessageQueueOptions
{
    /// <summary>
    /// Property name containg a value for session ID.
    /// If not specified non-sessioned mode is used.
    /// </summary>
    public string? SessionId { get; init; }
}
