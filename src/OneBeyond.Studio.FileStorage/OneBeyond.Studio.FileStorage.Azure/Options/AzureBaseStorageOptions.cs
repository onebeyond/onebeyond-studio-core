using System;

namespace OneBeyond.Studio.FileStorage.Azure.Options;

public abstract record AzureBaseStorageOptions
{
    public string? ConnectionString { get; init; }
    public string? ContainerName { get; init; }
    /// <summary>
    /// The duration of time, for which a shared access link to the file will be generated (if needed).
    /// </summary>
    public TimeSpan? SharedAccessDuration { get; init; }
}
