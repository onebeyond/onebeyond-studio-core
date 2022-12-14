using System.Collections.Generic;

namespace OneBeyond.Studio.Domain.SharedKernel.Tests.Testables;

internal sealed class TestableContainer<TItem>
{
    private readonly HashSet<TItem> _items;

    public TestableContainer()
    {
        _items = new HashSet<TItem>();
    }

    public IReadOnlyCollection<TItem> Items => _items;

    public void Add(TItem item)
        => _items.Add(item);

    public void Clear()
        => _items.Clear();
}
