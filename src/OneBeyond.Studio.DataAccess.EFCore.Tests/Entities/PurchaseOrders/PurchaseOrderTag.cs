using System;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

internal sealed class PurchaseOrderTag : DomainEntity<Guid>
{
    public PurchaseOrderTag(Guid purchaseOrderId, string description)
    {
        EnsureArg.IsNotDefault(purchaseOrderId, nameof(purchaseOrderId));
        EnsureArg.IsNotEmptyOrWhiteSpace(description, nameof(description));

        PurchaseOrderId = purchaseOrderId;
        Description = description;
    }

    public Guid PurchaseOrderId { get; private set; }
    public string Description { get; private set; }
}
