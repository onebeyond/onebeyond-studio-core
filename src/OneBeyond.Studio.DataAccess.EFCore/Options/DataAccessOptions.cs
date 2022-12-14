namespace OneBeyond.Studio.DataAccess.EFCore.Options;

public sealed record DataAccessOptions
{
    public bool EnableDetailedErrors { get; init; }
    public bool EnableSensitiveDataLogging { get; init; }
}
