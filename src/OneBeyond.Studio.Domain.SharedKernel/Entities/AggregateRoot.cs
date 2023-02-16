using System.Collections.Generic;
using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities;

public abstract class AggregateRoot<TEntityId>: DomainEntity<TEntityId>
{
    protected AggregateRoot(TEntityId entityId) : base(entityId)
    {
    }

    protected AggregateRoot() : base()
    {
    }

}

public abstract class AggregateRoot<TEntity, TEntityId>
    where TEntity : DomainEntity<TEntityId>
{
    private readonly List<TEntity> _entities = new List<TEntity>();

    public IReadOnlyCollection<TEntity> Entities => _entities.AsReadOnly();

    protected void AddEntity(TEntity entity)
    {
        EnsureArg.IsNotNull(entity, nameof(entity));

        _entities.Add(entity);
    }

    protected void RemoveEntity(TEntity entity)
    {
        EnsureArg.IsNotNull(entity, nameof(entity));

        _entities.Remove(entity);
    }
}
