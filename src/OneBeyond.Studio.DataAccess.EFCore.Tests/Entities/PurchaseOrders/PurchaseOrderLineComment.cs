using System;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

public sealed class PurchaseOrderLineComment : DomainEntity<Guid>
{
    internal PurchaseOrderLineComment(Guid purchaseOrderLineId, string text)
        : base(Guid.NewGuid())
    {
        EnsureArg.IsNotDefault(purchaseOrderLineId, nameof(purchaseOrderLineId));
        EnsureArg.IsNotNull(text, nameof(text));

        PurchaseOrderLineId = purchaseOrderLineId;
        Text = text;
        CreatedAt = DateTimeOffset.Now;
        IsArchived = false;
    }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    private PurchaseOrderLineComment()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    {
    }

    public Guid PurchaseOrderLineId { get; private set; }
    public string Text { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsArchived { get; private set; }

    public void Archive()
        => IsArchived = true;
}
