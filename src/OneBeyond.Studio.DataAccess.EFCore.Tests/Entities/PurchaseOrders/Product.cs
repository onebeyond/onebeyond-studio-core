using System;
using EnsureThat;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

public sealed class Product : DomainEntity<Guid>, IAggregateRoot
{
    public Product(
        string name,
        string type,
        string brand,
        decimal price,
        string countryOfOrigin)
        : base(Guid.NewGuid())
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
        EnsureArg.IsNotNullOrWhiteSpace(type, nameof(type));
        EnsureArg.IsNotNullOrWhiteSpace(brand, nameof(brand));
        EnsureArg.IsNotDefault(price, nameof(price));
        EnsureArg.IsNotNullOrWhiteSpace(countryOfOrigin, nameof(countryOfOrigin));

        Name = name;
        Type = type;
        Brand = brand;
        Price = price;
        CountryOfOrigin = countryOfOrigin;
    }

    public string Name { get; }
    public string Type { get; }
    public string Brand { get; }
    public decimal Price { get; }
    public string CountryOfOrigin { get; }
}
