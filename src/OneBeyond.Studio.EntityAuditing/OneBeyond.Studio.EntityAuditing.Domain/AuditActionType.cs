using Ardalis.SmartEnum;

namespace OneBeyond.Studio.EntityAuditing.Domain;

public sealed class AuditActionType : SmartEnum<AuditActionType, string>
{
    public static readonly AuditActionType Unchanged = new AuditActionType(nameof(Unchanged), nameof(Unchanged));
    public static readonly AuditActionType Insert = new AuditActionType(nameof(Insert), nameof(Insert));
    public static readonly AuditActionType Update = new AuditActionType(nameof(Update), nameof(Update));
    public static readonly AuditActionType Delete = new AuditActionType(nameof(Delete), nameof(Delete));

    private AuditActionType(string name, string value)
        : base(name, value)
    {
    }
}
