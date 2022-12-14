using System;
using System.Collections.Generic;
using System.Linq;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders.DomainEvents;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

internal sealed class PurchaseOrder : DomainEntity<Guid>, IAggregateRoot
{
    private readonly List<PurchaseOrderLine> _lines;
    private readonly List<PurchaseOrderTag> _tags;

    public PurchaseOrder()
        : base(Guid.NewGuid())
    {
        _lines = new List<PurchaseOrderLine>();
        _tags = new List<PurchaseOrderTag>();
    }

    public Vendor? Vendor { get; private set; }
    public IEnumerable<PurchaseOrderLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<PurchaseOrderTag> Tags => _tags.AsReadOnly();

    public void SetVendor(Vendor? vendor)
        => Vendor = vendor;

    public PurchaseOrderLine AddLine(string itemName)
    {
        if (_lines
            .Any((line) =>
                string.Compare(
                    line.ItemName,
                    itemName,
                    StringComparison.CurrentCultureIgnoreCase) == 0))
        {
            throw new Exception($"Purchase order line for item {itemName} already exists");
        }

        var purchaseOrderLine = new PurchaseOrderLine(Id, itemName);

        _lines.Add(purchaseOrderLine);

        RaiseDomainEvent(
            new PurchaseOrderLineAdded(
                Id,
                purchaseOrderLine.Id,
                DateTimeOffset.Now));

        return purchaseOrderLine;
    }

    public PurchaseOrderTag AddTag(string description)
    {
        if (_tags
            .Any((tag) =>
                string.Compare(
                    tag.Description,
                    description,
                    StringComparison.CurrentCultureIgnoreCase) == 0))
        {
            throw new Exception($"Purchase order tag {description} already exists");
        }

        var purchaseOrderTag = new PurchaseOrderTag(Id, description);

        _tags.Add(purchaseOrderTag);

        return purchaseOrderTag;
    }
}
