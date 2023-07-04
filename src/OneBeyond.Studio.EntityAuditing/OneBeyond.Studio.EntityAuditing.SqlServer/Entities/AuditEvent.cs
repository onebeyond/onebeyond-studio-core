using System;
using Audit.EntityFramework;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.EntityAuditing.Domain.AuditEventSerializers;

namespace OneBeyond.Studio.EntityAuditing.SqlServer.Entities;

[AuditIgnore]
public sealed class AuditEvent : DomainEntity<long>
{
    public string UserId { get; init; }
    public string EventType { get; init; }
    public string EntityId { get; init; }
    public string EntityName { get; init; }
    public string EntityDescription { get; init; }
    public DateTimeOffset InsertedDate { get; init; }
    public string ChangedData { get; init; }

    public static AuditEvent FromAuditInfo(
        Domain.AuditEvent auditEntityEvent,
        string entityName,
        string changesData)
        => new AuditEvent
            {
                UserId = auditEntityEvent.UserId,
                EventType = auditEntityEvent.EventType,
                InsertedDate = auditEntityEvent.InsertedDate,
                EntityId = auditEntityEvent.EntityId.ToString(),
                EntityName = entityName,
                EntityDescription = auditEntityEvent.EntityDescription,
                ChangedData = changesData
            };
}
