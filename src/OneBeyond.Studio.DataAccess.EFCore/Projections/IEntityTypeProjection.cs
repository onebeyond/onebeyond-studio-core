namespace OneBeyond.Studio.DataAccess.EFCore.Projections;

public interface IEntityTypeProjection { }

public interface IEntityTypeProjection<TEntity, TResult> : IEntityTypeProjection
    where TEntity : class
{
    public IQueryable<TResult> Project(IQueryable<TEntity> entityQuery, ProjectionContext context);
}
