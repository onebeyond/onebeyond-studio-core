using System;
using OneBeyond.Studio.Crosscuts.Strings;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

public sealed record AzureLargeMessageQueueOptions : AzureMessageQueueOptions
{
    public string? ContainerName { get; private set; }

    public override void EnsureIsValid()
    {
        base.EnsureIsValid();

        if (ContainerName.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(ContainerName), "Azure message queue container name is null.");
        }
    }
}
