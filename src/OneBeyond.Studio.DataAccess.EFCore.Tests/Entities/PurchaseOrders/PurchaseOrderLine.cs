using System;
using System.Collections.Generic;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

internal sealed class PurchaseOrderLine : DomainEntity<Guid>, IAggregateRoot
{
    private readonly List<PurchaseOrderLineComment> _comments;

    public PurchaseOrderLine(Guid purchaseOrderId, string itemName)
        : base(Guid.NewGuid())
    {
        EnsureArg.IsNotDefault(purchaseOrderId, nameof(purchaseOrderId));
        EnsureArg.IsNotEmptyOrWhiteSpace(itemName, nameof(itemName));

        _comments = new List<PurchaseOrderLineComment>();

        PurchaseOrderId = purchaseOrderId;
        ItemName = itemName;
    }

    public Guid PurchaseOrderId { get; private set; }
    public string ItemName { get; private set; }
    public Account? Account { get; private set; }
    public IEnumerable<PurchaseOrderLineComment> Comments => _comments.AsReadOnly();

    public PurchaseOrderLineComment AddComment(string text)
    {
        if (_comments.Count == 10)
        {
            throw new Exception("Purchase order line cannot have more than 10 comments");
        }
        var purchaseOrderLineComment = new PurchaseOrderLineComment(Id, text);
        _comments.Add(purchaseOrderLineComment);
        return purchaseOrderLineComment;
    }

    public void SetAccount(Account? account)
        => Account = account;
}
