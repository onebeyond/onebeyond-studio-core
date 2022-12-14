using System;
using OneBeyond.Studio.Crosscuts.Strings;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

public record AzureMessageQueueOptions
{
    public string? ConnectionString { get; init; }
    public string? QueueName { get; init; }

    public virtual void EnsureIsValid()
    {
        if (ConnectionString.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(ConnectionString), "Azure message queue connection string is null.");
        }

        if (QueueName.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(QueueName), "Azure message queue name is null.");
        }
    }
}
