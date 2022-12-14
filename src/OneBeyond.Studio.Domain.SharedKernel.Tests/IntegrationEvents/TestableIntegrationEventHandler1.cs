using System;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;
using OneBeyond.Studio.Domain.SharedKernel.Tests.Testables;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.IntegrationEvents;

internal sealed class TestableIntegrationEventHandler1
    : IIntegrationEventHandler<TestableIntegrationEvents.ThisHappened_1_1>
{
    private readonly TestableContainer<Type> _typeContainer;
    private readonly TestableContainer<TestableScopedItem> _scopedItemContainer;
    private readonly TestableScopedItem _scopedItem;

    public TestableIntegrationEventHandler1(
        TestableContainer<Type> typeContainer,
        TestableContainer<TestableScopedItem> scopedItemContainer,
        TestableScopedItem scopedItem)
    {
        EnsureArg.IsNotNull(typeContainer, nameof(typeContainer));
        EnsureArg.IsNotNull(scopedItemContainer, nameof(scopedItemContainer));
        EnsureArg.IsNotNull(scopedItem, nameof(scopedItem));

        _typeContainer = typeContainer;
        _scopedItemContainer = scopedItemContainer;
        _scopedItem = scopedItem;
    }

    public Task HandleAsync(
        TestableIntegrationEvents.ThisHappened_1_1 integrationEvent,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(integrationEvent, nameof(integrationEvent));

        _typeContainer.Add(GetType());
        _scopedItemContainer.Add(_scopedItem);
        _scopedItem.SetHandlerType(GetType());

        return Task.CompletedTask;
    }
}
