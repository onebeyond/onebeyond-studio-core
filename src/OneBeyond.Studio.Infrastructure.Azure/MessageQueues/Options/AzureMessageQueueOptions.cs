using System;
using OneBeyond.Studio.Crosscuts.Strings;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

public record AzureMessageQueueOptions
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
    /// The name of the queue to use.
    /// </summary>
    public string? QueueName { get; init; }

    public virtual void EnsureIsValid()
    {
        if (ConnectionString.IsNullOrWhiteSpace() && ResourceName.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("At least one connection must be provided, " +
                "either the connection string or the resource name (for Azure Identity usage).");
        }

        if (!ConnectionString.IsNullOrWhiteSpace() && !ResourceName.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Only one connection can be provided, " +
                "either the connection string or the resource name (for Azure Identity usage).");
        }

        if (QueueName.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(QueueName), "The Azure message queue name has not been provided.");
        }
    }
}
