using System;
using System.Collections.Generic;
using EnsureThat;

namespace OneBeyond.Studio.EntityAuditing.Domain;

public sealed class AuditEvent
{
    private readonly List<AuditChange> _entityChanges;
    private readonly List<AuditNavigationPropertyEvent> _navigationPropertyAuditEvents;

    public AuditEvent()
    {
        _entityChanges = new List<AuditChange>();
        _navigationPropertyAuditEvents = new List<AuditNavigationPropertyEvent>();
    }

    public DateTimeOffset InsertedDate { get; init; }
    public string EventType { get; init; }
    public string UserId { get; init; }
    public object EntityId { get; init; }
    public string EntityDescription { get; init; }
    public string AdditionalInformation { get; init; }
    public string Context { get; init; }

    public IEnumerable<AuditChange> EntityChanges
        => _entityChanges.AsReadOnly();

    public IEnumerable<AuditNavigationPropertyEvent> NavigationPropertyAuditEvents
        => _navigationPropertyAuditEvents.AsReadOnly();

    public void AddChange(AuditChange change)
    {
        EnsureArg.IsNotNull(change);
        _entityChanges.Add(change);
    }

    public void AddChanges(List<AuditChange> changes)
    {
        EnsureArg.IsNotNull(changes);
        _entityChanges.AddRange(changes);
    }

    public void RemoveChangeByPropertyName(string propertyName)
    {
        _entityChanges.RemoveAll(c => c.PropertyName.Equals(propertyName));
    }

    public void AddNavigationAuditEvent(AuditNavigationPropertyEvent change)
    {
        EnsureArg.IsNotNull(change);
        _navigationPropertyAuditEvents.Add(change);
    }

    public void RemoveNavigationAuditEventByPropertyName(string propertyName)
    {
        _navigationPropertyAuditEvents.RemoveAll(c => c.PropertyName.Equals(propertyName));
    }
}
