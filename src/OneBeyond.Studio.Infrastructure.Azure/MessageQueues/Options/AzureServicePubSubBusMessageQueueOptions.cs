using System;
using OneBeyond.Studio.Crosscuts.Strings;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

public record AzureServicePubSubBusMessageQueueOptions
{
    public string? ResourceName { get; init; }
    public string? ConnectionString { get; init; }
    public string? TopicName { get; init; }

    public virtual void EnsureIsValid()
    {
        if (ConnectionString.IsNullOrWhiteSpace() && ResourceName.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(ConnectionString), "At least one connection must be provided, " +
                "either the connection string or the namespace name (for Managed Identity usage).");
        }

        if (!ConnectionString.IsNullOrWhiteSpace() && !ResourceName.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(ConnectionString), "Only one connection can be provided, " +
                "either the connection string or the namespace name (for Managed Identity usage).");
        }

        if (TopicName.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(TopicName), "The Azure topic name has not been provided.");
        }
    }
}
