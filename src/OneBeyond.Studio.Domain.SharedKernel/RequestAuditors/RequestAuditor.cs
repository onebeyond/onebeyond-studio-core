using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using MediatR;

namespace OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

internal sealed class RequestAuditor<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuditableRequest<TResponse>
{
    private readonly IRequestAuditRecordBuilder<TRequest> _requestAuditRecordBuilder;
    private readonly IReadOnlyList<IRequestAuditRecordWriter<TRequest>> _requestAuditRecordWriters;

    public RequestAuditor(
        IRequestAuditRecordBuilder<TRequest> requestAuditRecordBuilder,
        IEnumerable<IRequestAuditRecordWriter<TRequest>> requestAuditRecordWriters)
    {
        EnsureArg.IsNotNull(requestAuditRecordBuilder, nameof(requestAuditRecordBuilder));
        EnsureArg.IsNotNull(requestAuditRecordWriters, nameof(requestAuditRecordWriters));

        _requestAuditRecordBuilder = requestAuditRecordBuilder;
        _requestAuditRecordWriters = requestAuditRecordWriters.ToList();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next().ConfigureAwait(false);

        var requestAuditRecord = await _requestAuditRecordBuilder.BuildAsync(request, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(requestAuditRecord))
        {
            foreach (var requestAuditRecordWriter in _requestAuditRecordWriters)
            {
                await requestAuditRecordWriter.WriteAsync(requestAuditRecord, cancellationToken).ConfigureAwait(false);
            }
        }

        return response;
    }
}
