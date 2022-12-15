using System;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders.DomainEvents;

public sealed class PurchaseOrderLineAdded : DomainEvent
{
    public PurchaseOrderLineAdded(
        Guid purchaseOrderId,
        Guid purchaseOrderLineId,
        DateTimeOffset dateRaised)
        : base(dateRaised)
    {
        EnsureArg.IsNotDefault(purchaseOrderId, nameof(purchaseOrderId));
        EnsureArg.IsNotDefault(purchaseOrderLineId, nameof(purchaseOrderLineId));
        PurchaseOrderId = purchaseOrderId;
        PurchaseOrderLineId = purchaseOrderLineId;
    }

    public Guid PurchaseOrderId { get; }
    public Guid PurchaseOrderLineId { get; }
}
