using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.DomainEvents;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders.DomainEvents;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.Services;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders.DomainEventHandlers;

internal sealed class PurchaseOrderLineAddedSmsNotifier
    : IPostSaveDomainEventHandler<PurchaseOrderLineAdded>
{
    private readonly Container<PostSaveScopedItem> _container;
    private readonly PostSaveScopedItem _scopedItem;

    public PurchaseOrderLineAddedSmsNotifier(
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
