using System;
using OneBeyond.Studio.Crosscuts.Strings;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

public record AzureServicePubSubBusMessageQueueOptions
{
    public string? ConnectionString { get; init; }
    public string? TopicName { get; init; }

    public virtual void EnsureIsValid()
    {
        if (ConnectionString.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(ConnectionString), "Azure topic connection string is null.");
        }

        if (TopicName.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(TopicName), "Azure topic name is null.");
        }
    }
}
