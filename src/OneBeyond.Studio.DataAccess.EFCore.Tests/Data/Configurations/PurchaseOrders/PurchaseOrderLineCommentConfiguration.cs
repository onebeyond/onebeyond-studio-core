using System;
using OneBeyond.Studio.DataAccess.EFCore.Configurations;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Configurations.PurchaseOrders;

internal sealed class PurchaseOrderLineCommentConfiguration : BaseEntityTypeConfiguration<PurchaseOrderLineComment, Guid>
{
}
