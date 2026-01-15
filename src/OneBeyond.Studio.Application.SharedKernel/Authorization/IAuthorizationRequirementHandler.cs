using System.Threading;
using System.Threading.Tasks;
using OneBeyond.Studio.Core.Mediator;
using OneBeyond.Studio.Domain.SharedKernel.Authorization;

namespace OneBeyond.Studio.Application.SharedKernel.Authorization;

public interface IAuthorizationRequirementHandler<in TRequirement, in TRequest>
    where TRequirement : AuthorizationRequirement
    where TRequest : IBaseRequest
{
    public Task HandleAsync(TRequirement requirement, TRequest request, CancellationToken cancellationToken);
}
