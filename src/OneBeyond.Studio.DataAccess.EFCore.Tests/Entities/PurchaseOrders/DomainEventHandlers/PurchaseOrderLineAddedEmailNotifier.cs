using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders.DomainEvents;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.Services;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders.DomainEventHandlers;

internal sealed class PurchaseOrderLineAddedEmailNotifier
    : IPostSaveDomainEventHandler<PurchaseOrderLineAdded>
{
    private readonly Container<PostSaveScopedItem> _container;
    private readonly PostSaveScopedItem _scopedItem;

    public PurchaseOrderLineAddedEmailNotifier(
        Container<PostSaveScopedItem> container,
        PostSaveScopedItem scopedItem)
    {
        EnsureArg.IsNotNull(container, nameof(container));
        EnsureArg.IsNotNull(scopedItem, nameof(scopedItem));

        _container = container;
        _scopedItem = scopedItem;
    }

    public Task HandleAsync(
        PurchaseOrderLineAdded domainEvent,
        IReadOnlyDictionary<string, object> domainEventAmbientContext,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(domainEvent, nameof(domainEvent));

        _container.Add(_scopedItem);

        return Task.CompletedTask;
    }
}
