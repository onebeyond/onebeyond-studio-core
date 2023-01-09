using System;
using EnsureThat;

namespace OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

internal sealed record IntegrationEventType
{
    public IntegrationEventType(
        string typeName,
        int majorVersion,
        int minorVersion,
        Type clrType)
    {
        EnsureArg.IsNotNullOrWhiteSpace(typeName, nameof(typeName));
        EnsureArg.IsGt(majorVersion, 0, nameof(majorVersion));
        EnsureArg.IsGte(minorVersion, 0, nameof(minorVersion));
        EnsureArg.IsNotNull(clrType, nameof(clrType));

        TypeName = typeName;
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
        ClrType = clrType;
    }

    public string TypeName { get; }
    public int MajorVersion { get; }
    public int MinorVersion { get; }
    public Type ClrType { get; }
    public decimal Version => MajorVersion + (decimal)MinorVersion / 10;
}
