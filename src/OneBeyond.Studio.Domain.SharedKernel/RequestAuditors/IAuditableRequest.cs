using OneBeyond.Studio.Core.Mediator.Commands;

namespace OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

/// <summary>
/// </summary>
public interface IAuditableRequest<out TResponse> : ICommand<TResponse>
{
}
