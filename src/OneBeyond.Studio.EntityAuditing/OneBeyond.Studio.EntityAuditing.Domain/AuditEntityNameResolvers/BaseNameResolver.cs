namespace OneBeyond.Studio.EntityAuditing.Domain.AuditEntityNameResolvers;

public abstract class BaseNameResolver<TEntity> : IAuditNameResolver<TEntity>
    where TEntity : class
{
    public virtual string GetEntityName()
        => typeof(TEntity).Name;
}
