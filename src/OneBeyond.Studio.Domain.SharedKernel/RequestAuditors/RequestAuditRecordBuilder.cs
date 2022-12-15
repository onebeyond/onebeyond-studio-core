using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

internal sealed class RequestAuditRecordBuilder<TRequest> : IRequestAuditRecordBuilder<TRequest>
{
    public ValueTask<string> BuildAsync(TRequest request, CancellationToken cancellationToken)
        => new ValueTask<string>();
}
