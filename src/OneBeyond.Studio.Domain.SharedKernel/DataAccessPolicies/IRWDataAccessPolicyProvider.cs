using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Domain.SharedKernel.DataAccessPolicies;

/// <summary>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IRWDataAccessPolicyProvider<TEntity> : IRODataAccessPolicyProvider<TEntity>
{
    /// <summary>
    /// Create data access policy for an entity. Null value grants the access.
    /// </summary>
    Task<DataAccessPolicy<TEntity>?> GetCreateDataAccessPolicyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update data access policy for an entity. Null value grants the access.
    /// </summary>
    Task<DataAccessPolicy<TEntity>?> GetUpdateDataAccessPolicyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete data access policy for an entity. Null value grants the access.
    /// </summary>
    Task<DataAccessPolicy<TEntity>?> GetDeleteDataAccessPolicyAsync(CancellationToken cancellationToken = default);
}
