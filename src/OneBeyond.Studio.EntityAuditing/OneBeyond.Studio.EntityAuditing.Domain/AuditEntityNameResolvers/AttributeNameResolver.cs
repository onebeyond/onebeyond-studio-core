using System;
using OneBeyond.Studio.EntityAuditing.Domain.Attributes;

namespace OneBeyond.Studio.EntityAuditing.Domain.AuditEntityNameResolvers;

public sealed class AttributeNameResolver<TEntity> : BaseNameResolver<TEntity>
    where TEntity : class
{
    public override string GetEntityName()
    {
        var type = Attribute.GetCustomAttribute(typeof(TEntity), typeof(AuditNameAttribute));
        return type != null
            ? ((AuditNameAttribute)type).Name
            : base.GetEntityName();
    }
}
