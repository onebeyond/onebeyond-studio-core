using System;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

public sealed class Vendor : AggregateRoot<Guid>
{
    public Vendor(string name)
        : base(Guid.NewGuid())
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public string Name { get; private set; }
}
