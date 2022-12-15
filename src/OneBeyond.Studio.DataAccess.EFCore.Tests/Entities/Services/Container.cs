using System.Collections.Generic;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.Services;

internal sealed class Container<TItem>
{
    private readonly HashSet<TItem> _items;

    public Container()
    {
        _items = new HashSet<TItem>();
    }

    public IReadOnlyCollection<TItem> Items => _items;

    public void Add(TItem item)
        => _items.Add(item);

    public void Clear()
        => _items.Clear();
}
