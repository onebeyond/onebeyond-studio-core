using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Application.SharedKernel.DataAccessPolicies;

/// <summary>
/// Data access policy provider implementation which always grants access to any entity.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public sealed class AllowDataAccessPolicyProvider<TEntity> : IRWDataAccessPolicyProvider<TEntity>
{
    /// <summary>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<DataAccessPolicy<TEntity>?> GetCreateDataAccessPolicyAsync(CancellationToken cancellationToken)
        => Task.FromResult(default(DataAccessPolicy<TEntity>?));

    /// <summary>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<DataAccessPolicy<TEntity>?> GetUpdateDataAccessPolicyAsync(CancellationToken cancellationToken)
        => Task.FromResult(default(DataAccessPolicy<TEntity>?));

    /// <summary>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<DataAccessPolicy<TEntity>?> GetDeleteDataAccessPolicyAsync(CancellationToken cancellationToken)
        => Task.FromResult(default(DataAccessPolicy<TEntity>?));

    /// <summary>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<DataAccessPolicy<TEntity>?> GetReadDataAccessPolicyAsync(CancellationToken cancellationToken)
        => Task.FromResult(default(DataAccessPolicy<TEntity>?));
}
