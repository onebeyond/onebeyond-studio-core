using Autofac.Features.OwnedInstances;
using Microsoft.EntityFrameworkCore;

namespace OneBeyond.Studio.DataAccess.EFCore.DomainEvents;

internal sealed class MsSQLRaisedDomainEventReceiver<TDbContext>
    : RelationalDbRaisedDomainEventReceiver<TDbContext>
    where TDbContext : DbContext
{
    public MsSQLRaisedDomainEventReceiver(Owned<TDbContext> dbContext)
        : base(dbContext)
    {
    }

    protected override string SelectForUpdateSql =>
$@"
select top(1) *
from {RaisedDomainEventConfiguration.RaisedDomainEventTableName}
with (xlock, rowlock, readpast)
";
    protected override string DeleteByIdSql =>
$@"
delete from {RaisedDomainEventConfiguration.RaisedDomainEventTableName}
where {RaisedDomainEventConfiguration.RaisedDomainEventIdColumnName} = {{0}}
";
}
