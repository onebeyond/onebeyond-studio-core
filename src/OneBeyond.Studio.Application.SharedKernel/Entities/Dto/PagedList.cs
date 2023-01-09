using System.Collections.Generic;

namespace OneBeyond.Studio.Application.SharedKernel.Entities.Dto;

/// <summary>
/// List of entities with their total count
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public struct PagedList<TEntity>
{
    /// <summary>
    /// Total number of items avaialable.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Requested page items.
    /// </summary>
    public IReadOnlyCollection<TEntity> Data { get; set; }
}
