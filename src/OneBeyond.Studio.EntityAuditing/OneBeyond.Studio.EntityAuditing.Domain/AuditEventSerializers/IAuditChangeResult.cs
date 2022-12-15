namespace OneBeyond.Studio.EntityAuditing.Domain.AuditEventSerializers;

/// <summary>
/// List of entities with their total count
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IAuditChangeResult<out TResult>
{
    /// <summary>
    /// True if no serialized data found.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Serialized data.
    /// </summary>
    TResult Data { get; }
}
