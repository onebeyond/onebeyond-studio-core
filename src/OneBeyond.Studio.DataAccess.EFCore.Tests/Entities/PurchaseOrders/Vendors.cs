using System;
using System.Linq;
using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

public sealed class Vendors : AggregateRoot<Vendor, Guid>
{
    public Vendor AddVendor(string name)
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));

        EnsureNameUnique(null, name);

        return AddEntity(new Vendor(name));
    }

    public Vendor UpdateVendor(Guid id, string name)
    {
        EnsureArg.IsNotDefault(id, nameof(id));
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));

        var entity = Entities.SingleOrDefault(x => x.Id == id)
            ?? throw new ValidationException("Vendor with the id not found");

        EnsureNameUnique(id, name);

        entity.UpdateName(name);

        return entity;
    }

    private void EnsureNameUnique(Guid? id, string name)
    {
        if (Entities.Any(vendor => vendor.Name == name && vendor.Id != id))
        {
            throw new ValidationException("Vendor with the same name already exists");
        }
    }
}
