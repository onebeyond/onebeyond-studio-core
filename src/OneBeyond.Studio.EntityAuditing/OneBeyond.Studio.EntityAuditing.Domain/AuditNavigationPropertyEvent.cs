using System;
using System.Collections.Generic;
using EnsureThat;

namespace OneBeyond.Studio.EntityAuditing.Domain;

public sealed class AuditNavigationPropertyEvent
{
    private readonly List<AuditChange> _relatedChanges;

    public AuditNavigationPropertyEvent()
        => _relatedChanges = new List<AuditChange>();

    public string PropertyName { get; set; }
    public string EventType { get; set; }
    public object EntityId { get; set; }
    public Type EntityType { get; set; }
    public IEnumerable<AuditChange> RelatedChanges
        => _relatedChanges.AsReadOnly();

    public void AddChange(AuditChange change)
    {
        EnsureArg.IsNotNull(change);
        _relatedChanges.Add(change);
    }

    public void AddChanges(List<AuditChange> changes)
    {
        EnsureArg.IsNotNull(changes);
        _relatedChanges.AddRange(changes);
    }

    public void RemoveChangeByPropertyName(string propertyName)
    {
        _relatedChanges.RemoveAll(c => c.PropertyName.Equals(propertyName));
    }
}
