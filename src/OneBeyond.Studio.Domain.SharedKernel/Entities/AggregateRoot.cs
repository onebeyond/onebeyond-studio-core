using System.Collections.Generic;
using EnsureThat;

namespace OneBeyond.Studio.Domain.SharedKernel.Entities;

/// <summary>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TEntityId"></typeparam>
public abstract class AggregateRoot<TEntity, TEntityId> : IAggregateRoot
    where TEntity : DomainEntity<TEntityId>
{
    private readonly List<TEntity> _entities = new List<TEntity>();

    /// <summary>
    /// </summary>
    public IReadOnlyCollection<TEntity> Entities => _entities.AsReadOnly();

    /// <summary>
    /// </summary>
    /// <param name="entity"></param>
    protected void AddEntity(TEntity entity)
    {
        EnsureArg.IsNotNull(entity, nameof(entity));

        _entities.Add(entity);
    }

    /// <summary>
    /// </summary>
    /// <param name="entity"></param>
    protected void RemoveEntity(TEntity entity)
    {
        EnsureArg.IsNotNull(entity, nameof(entity));

        _entities.Remove(entity);
    }
}
