namespace OneBeyond.Studio.Infrastructure.Azure.KeyVault.Options;

public sealed record KeyVaultOptions
{
    public bool Enabled { get; init; }

    public string? Name { get; init; }
}
