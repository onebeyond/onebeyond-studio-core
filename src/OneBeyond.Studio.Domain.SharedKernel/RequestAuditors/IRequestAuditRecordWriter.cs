using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

/// <summary>
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public interface IRequestAuditRecordWriter<TRequest>
{
    /// <summary>
    /// </summary>
    /// <param name="requestAuditRecord"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WriteAsync(string requestAuditRecord, CancellationToken cancellationToken = default);
}
