using System;
using Audit.EntityFramework;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.EntityAuditing.SqlServer.Entities;

[AuditIgnore]
public sealed class AuditEvent : DomainEntity<int>
{
    public string UserId { get; set; }
    public string EventType { get; set; }
    public string EntityId { get; set; }
    public string EntityName { get; set; }
    public string EntityDescription { get; set; }
    public DateTimeOffset InsertedDate { get; set; }
    public string ChangedData { get; set; }

    public static AuditEvent FromAuditInfo(Domain.AuditEvent auditEntityEvent)
    {
        return new AuditEvent()
        {
            UserId = auditEntityEvent.UserId,
            EventType = auditEntityEvent.EventType,
            InsertedDate = auditEntityEvent.InsertedDate,
            EntityId = auditEntityEvent.EntityId.ToString(),
            EntityDescription = auditEntityEvent.EntityDescription
        };
    }
}
