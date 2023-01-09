using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneBeyond.Studio.DataAccess.EFCore.Configurations;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Configurations.PurchaseOrders;

internal sealed class PurchaseOrderConfiguration : BaseEntityTypeConfiguration<PurchaseOrder, Guid>
{
    protected override void DoConfigure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasOne((e) => e.Vendor)
            .WithMany();

        builder.HasMany((e) => e.Lines)
            .WithOne()
            .IsRequired()
            .HasForeignKey((e) => e.PurchaseOrderId);

        builder.HasMany((e) => e.Tags)
            .WithOne()
            .IsRequired()
            .HasForeignKey((e) => e.PurchaseOrderId);
    }
}
