using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OneBeyond.Studio.Domain.SharedKernel.Authorization;

namespace OneBeyond.Studio.Application.SharedKernel.Authorization;

/// <summary>
/// </summary>
/// <typeparam name="TRequirement"></typeparam>
/// <typeparam name="TRequest"></typeparam>
public interface IAuthorizationRequirementHandler<in TRequirement, in TRequest>
    where TRequirement : AuthorizationRequirement
    where TRequest : IBaseRequest
{
    /// <summary>
    /// </summary>
    /// <param name="requirement"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TRequirement requirement, TRequest request, CancellationToken cancellationToken);
}
