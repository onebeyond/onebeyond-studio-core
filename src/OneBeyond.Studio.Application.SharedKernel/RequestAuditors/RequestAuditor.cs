using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Core.Mediator.Pipelines;
using OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

namespace OneBeyond.Studio.Application.SharedKernel.RequestAuditors;

internal sealed class RequestAuditor<TRequest, TResponse> : IMediatorPipelineBehaviour<TRequest, TResponse>
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

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
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
