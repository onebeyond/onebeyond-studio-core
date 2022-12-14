using System;
using System.Runtime.Serialization;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Domain.SharedKernel.Repositories.Exceptions;

/// <summary>
/// </summary>
[Serializable]
public abstract class EntityNotFoundException : OneBeyondException
{
    /// <summary>
    /// </summary>
    protected EntityNotFoundException()
        : base()
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    protected EntityNotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    protected EntityNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected EntityNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

/// <summary>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TEntityId"></typeparam>
[Serializable]
public sealed class EntityNotFoundException<TEntity, TEntityId> : EntityNotFoundException
    where TEntity : DomainEntity<TEntityId>
{
    /// <summary>
    /// </summary>
    public EntityNotFoundException()
        : base("Entity is not found")
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="entityId"></param>
    public EntityNotFoundException(TEntityId entityId)
        : base($"Entity #{entityId} is not found")
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    private EntityNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
