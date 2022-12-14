using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneBeyond.Studio.DataAccess.EFCore.Configurations;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Configurations;

internal sealed class ProductConfiguration : BaseEntityTypeConfiguration<Product, Guid>
{
    protected override void DoConfigure(EntityTypeBuilder<Product> builder)
    {
        builder.Property((product) => product.Name);
        builder.Property((product) => product.Brand);
        builder.Property((product) => product.Type);
        builder.Property((product) => product.Price);
        builder.Property((product) => product.CountryOfOrigin);
    }
}
