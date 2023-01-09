using OneBeyond.Studio.Application.SharedKernel.DataAccessPolicies;
using OneBeyond.Studio.DataAccess.EFCore.Projections;
using OneBeyond.Studio.DataAccess.EFCore.Repositories;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests.Data.Repositories;

internal class RWRepository<TAggregateRoot, TAggregateRootId>
    : BaseRWRepository<DbContexts.DbContext, TAggregateRoot, TAggregateRootId>
    where TAggregateRoot : DomainEntity<TAggregateRootId>, IAggregateRoot
    where TAggregateRootId : notnull
{
    public RWRepository(
        DbContexts.DbContext dbContext,
        IEntityTypeProjections<TAggregateRoot> entityTypeProjections)
        : base(
              dbContext,
              new AllowDataAccessPolicyProvider<TAggregateRoot>(),
              entityTypeProjections)
    {
    }
}
