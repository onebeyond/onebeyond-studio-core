using System;
using OneBeyond.Studio.Crosscuts.Strings;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

public record AzureServicePubSubBusMessageQueueOptions
{
    /// <summary>
    /// The name of the Azure resource to use for Azure Identity authentication.
    /// NOTE: If specified, a connection string must not be provided.
    /// </summary>
    public string? ResourceName { get; init; }

    /// <summary>
    /// The connection string to use for authentication.
    /// NOTE: if specified, a resource name must not be provided.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// The name of the topic to use.
    /// </summary>
    public string? TopicName { get; init; }

    public virtual void EnsureIsValid()
    {
        if (ConnectionString.IsNullOrWhiteSpace() && ResourceName.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("At least one connection must be provided, " +
                "either the connection string or the namespace name (for Azure Identity usage).");
        }

        if (!ConnectionString.IsNullOrWhiteSpace() && !ResourceName.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Only one connection can be provided, " +
                "either the connection string or the namespace name (for Azure Identity usage).");
        }

        if (TopicName.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(TopicName), "The Azure topic name has not been provided.");
        }
    }
}
