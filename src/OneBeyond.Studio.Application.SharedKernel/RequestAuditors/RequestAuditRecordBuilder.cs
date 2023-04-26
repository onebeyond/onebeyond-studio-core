using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Application.SharedKernel.RequestAuditors;

internal sealed class RequestAuditRecordBuilder<TRequest> : IRequestAuditRecordBuilder<TRequest>
{
    public ValueTask<string> BuildAsync(TRequest request, CancellationToken cancellationToken = default)
        => new();
}
