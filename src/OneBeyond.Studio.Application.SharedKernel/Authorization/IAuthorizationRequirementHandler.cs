using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Core.Mediator.Commands;
using OneBeyond.Studio.Domain.SharedKernel.Authorization;

namespace OneBeyond.Studio.Application.SharedKernel.Authorization;

/// <summary>
/// </summary>
/// <typeparam name="TRequirement"></typeparam>
/// <typeparam name="TRequest"></typeparam>
public interface IAuthorizationRequirementHandler<in TRequirement, in TRequest>
    where TRequirement : AuthorizationRequirement
    where TRequest : ICommand
{
    /// <summary>
    /// </summary>
    /// <param name="requirement"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task HandleAsync(TRequirement requirement, TRequest request, CancellationToken cancellationToken);
}
