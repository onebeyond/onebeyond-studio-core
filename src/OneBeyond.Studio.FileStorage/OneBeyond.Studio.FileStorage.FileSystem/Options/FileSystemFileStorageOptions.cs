namespace OneBeyond.Studio.FileStorage.FileSystem.Options;

public sealed record FileSystemFileStorageOptions
{
    public string StorageRootPath { get; init; }
    public bool AllowDownloadUrl { get; init; }
}
