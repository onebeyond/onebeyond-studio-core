using OneBeyond.Studio.DataAccess.EFCore.Configurations;
using OneBeyond.Studio.EntityAuditing.SqlServer.Entities;

namespace OneBeyond.Studio.EntityAuditing.SqlServer.Configuration;

internal sealed class AuditEventConfiguration : BaseEntityTypeConfiguration<AuditEvent, long>
{
}
