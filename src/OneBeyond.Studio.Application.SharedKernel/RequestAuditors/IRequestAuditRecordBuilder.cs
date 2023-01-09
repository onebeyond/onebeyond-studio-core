using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Application.SharedKernel.RequestAuditors;

/// <summary>
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public interface IRequestAuditRecordBuilder<TRequest>
{
    /// <summary>
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<string> BuildAsync(TRequest request, CancellationToken cancellationToken = default);
}
