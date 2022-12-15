namespace OneBeyond.Studio.EntityAuditing.Domain.AuditEntityNameResolvers;

public interface IAuditNameResolver<TEntity>
    where TEntity : class
{
    string GetEntityName();
}
