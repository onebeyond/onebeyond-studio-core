using MediatR;

namespace OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

/// <summary>
/// </summary>
public interface IAuditableRequest<out TResponse> : IRequest<TResponse>
{
}
