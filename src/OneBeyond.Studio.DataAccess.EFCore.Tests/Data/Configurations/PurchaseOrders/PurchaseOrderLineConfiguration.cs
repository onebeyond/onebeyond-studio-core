using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneBeyond.Studio.DataAccess.EFCore.Configurations;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Configurations.PurchaseOrders;

internal sealed class PurchaseOrderLineConfiguration : BaseEntityTypeConfiguration<PurchaseOrderLine, Guid>
{
    protected override void DoConfigure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.HasMany((e) => e.Comments)
            .WithOne()
            .IsRequired()
            .HasForeignKey((e) => e.PurchaseOrderLineId);

        builder.HasOne((e) => e.Account)
            .WithMany();
    }
}
