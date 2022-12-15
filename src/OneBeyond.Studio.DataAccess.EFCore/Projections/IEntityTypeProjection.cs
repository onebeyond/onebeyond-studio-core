using System.Linq;

namespace OneBeyond.Studio.DataAccess.EFCore.Projections;

public interface IEntityTypeProjection<TEntity>
    where TEntity : class
{
}

public interface IEntityTypeProjection<TEntity, TResult> : IEntityTypeProjection<TEntity>
    where TEntity : class
{
    IQueryable<TResult> Project(IQueryable<TEntity> entityQuery, ProjectionContext context);
}
