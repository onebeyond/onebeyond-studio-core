using OneBeyond.Studio.DataAccess.EFCore.Projections;
using OneBeyond.Studio.DataAccess.EFCore.Repositories;
using OneBeyond.Studio.Domain.SharedKernel.DataAccessPolicies;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Repositories;

internal class RORepository<TEntity, TEntityId>
    : BaseRORepository<DbContexts.DbContext, TEntity, TEntityId>
    where TEntity : DomainEntity<TEntityId>
    where TEntityId : notnull
{
    public RORepository(
        DbContexts.DbContext dbContext,
        IEntityTypeProjections<TEntity> entityTypeProjections)
        : base(
              dbContext,
              new AllowDataAccessPolicyProvider<TEntity>(),
              entityTypeProjections)
    {
    }
}
