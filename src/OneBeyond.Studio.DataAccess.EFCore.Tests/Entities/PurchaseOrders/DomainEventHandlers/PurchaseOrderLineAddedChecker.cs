using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders.DomainEvents;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.Services;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders.DomainEventHandlers;

internal sealed class PurchaseOrderLineAddedChecker
    : IPreSaveDomainEventHandler<PurchaseOrderLineAdded>
{
    private readonly Container<PreSaveScopedItem> _container;
    private readonly PreSaveScopedItem _preSaveScopedItem;

    public PurchaseOrderLineAddedChecker(
        Container<PreSaveScopedItem> container,
        PreSaveScopedItem preSaveScopedItem)
    {
        EnsureArg.IsNotNull(container, nameof(container));
        EnsureArg.IsNotNull(preSaveScopedItem, nameof(preSaveScopedItem));

        _container = container;
        _preSaveScopedItem = preSaveScopedItem;
    }

    public Task HandleAsync(
        PurchaseOrderLineAdded domainEvent,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(domainEvent, nameof(domainEvent));

        _container.Add(_preSaveScopedItem);

        return Task.CompletedTask;
    }
}
