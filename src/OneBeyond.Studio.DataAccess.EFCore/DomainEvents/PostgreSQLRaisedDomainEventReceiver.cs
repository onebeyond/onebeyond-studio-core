using Autofac.Features.OwnedInstances;
using Microsoft.EntityFrameworkCore;

namespace OneBeyond.Studio.DataAccess.EFCore.DomainEvents;

internal sealed class PostgreSQLRaisedDomainEventReceiver<TDbContext>
    : RelationalDbRaisedDomainEventReceiver<TDbContext>
    where TDbContext : DbContext
{
    public PostgreSQLRaisedDomainEventReceiver(Owned<TDbContext> dbContext)
        : base(dbContext)
    {
    }

    protected override string SelectForUpdateSql =>
$@"
select *
from ""{RaisedDomainEventConfiguration.RaisedDomainEventTableName}""
order by ""{RaisedDomainEventConfiguration.RaisedDomainEventIdColumnName}""
for update skip locked
limit 1
";
    protected override string DeleteByIdSql =>
$@"
delete from ""{RaisedDomainEventConfiguration.RaisedDomainEventTableName}""
where ""{RaisedDomainEventConfiguration.RaisedDomainEventIdColumnName}"" = {{0}}
";
}
