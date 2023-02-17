using OneBeyond.Studio.DataAccess.EFCore.Repositories;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Repositories;

internal sealed class AggregateRootRWRepository<TAggregateRoot, TEntity, TEntityId>
    : AggregateRootRWRepository<DbContexts.DbContext, TAggregateRoot, TEntity, TEntityId>
    where TAggregateRoot : AggregateRoot<TEntity, TEntityId>
    where TEntity : DomainEntity<TEntityId>
{
    public AggregateRootRWRepository(DbContexts.DbContext dbContext)
        : base(dbContext)
    {
    }
}
