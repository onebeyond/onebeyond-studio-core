using System;
using System.Runtime.Serialization;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Domain.SharedKernel.Entities;

namespace OneBeyond.Studio.Domain.SharedKernel.Repositories.Exceptions;

/// <summary>
/// </summary>
[Serializable]
public abstract class EntityAccessDeniedException : OneBeyondException
{
    /// <summary>
    /// </summary>
    protected EntityAccessDeniedException()
        : base()
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    protected EntityAccessDeniedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    protected EntityAccessDeniedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected EntityAccessDeniedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

/// <summary>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TEntityId"></typeparam>
[Serializable]
public sealed class EntityAccessDeniedException<TEntity, TEntityId> : EntityAccessDeniedException
    where TEntity : DomainEntity<TEntityId>
{
    /// <summary>
    /// </summary>
    public EntityAccessDeniedException()
        : base("Access to entity is denied")
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="entityId"></param>
    public EntityAccessDeniedException(TEntityId entityId)
        : base($"Access to entity #{entityId} is denied")
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    private EntityAccessDeniedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
