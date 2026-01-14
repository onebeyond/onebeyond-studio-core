using OneBeyond.Studio.Core.Mediator;

namespace OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

/// <summary>
/// </summary>
public interface IAuditableRequest<out TResponse> : IRequest<TResponse>
{
}
