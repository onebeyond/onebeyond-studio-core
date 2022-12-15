namespace OneBeyond.Studio.Domain.SharedKernel.Entities;

/// <summary>
/// Interface for entities supporting soft deletion.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Marks entity as soft deleted.
    /// </summary>
    void MarkAsDeleted();
}
