using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Application.SharedKernel.DataAccessPolicies;

/// <summary>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IRODataAccessPolicyProvider<TEntity>
{
    /// <summary>
    /// Read data access policy for an entity. Null value grants the access.
    /// </summary>
    Task<DataAccessPolicy<TEntity>?> GetReadDataAccessPolicyAsync(CancellationToken cancellationToken = default);
}
