using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Exceptions;

namespace OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

internal static class IntegrationEventVersion
{
    public static string ToString(int major, int minor)
        => $"{major}.{minor}";

    public static (int Major, int Minor) FromString(string version)
    {
        EnsureArg.IsNotNullOrWhiteSpace(version, nameof(version));

        var versionParts = version.Split('.');

        if (versionParts.Length != 2)
        {
            throw new IntegrationEventException(
                $"Integration event version {version} is in invalid format.");
        }

        return !int.TryParse(versionParts[0], out var major)
            || !int.TryParse(versionParts[1], out var minor)
            ? throw new IntegrationEventException(
                $"Integration event version {version} is in invalid format.")
            : (major, minor);
    }
}
