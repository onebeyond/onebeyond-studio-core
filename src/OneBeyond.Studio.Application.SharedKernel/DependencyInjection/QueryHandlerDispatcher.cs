using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using EnsureThat;
using OneBeyond.Studio.Core.Mediator.Queries;

namespace OneBeyond.Studio.Application.SharedKernel.DependencyInjection;

/// <summary>
/// Dispatches generic query handlers with use of Autofac keyed registrations
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class QueryHandlerDispatcher<TRequest, TResponse> : IQueryHandler<TRequest, TResponse>
    where TRequest : IQuery<TResponse>
{
    private readonly IIndex<Type, IQueryHandler<TRequest, TResponse>> _requestHandlers;
    /// <summary>
    /// </summary>
    public QueryHandlerDispatcher(IIndex<Type, IQueryHandler<TRequest, TResponse>> requestHandlers)
    {
        EnsureArg.IsNotNull(requestHandlers, nameof(requestHandlers));

        _requestHandlers = requestHandlers;
    }
    public Task<TResponse> HandleAsync(TRequest query, CancellationToken cancellationToken = default)
    {
        var requestHandler = _requestHandlers[RequestTypeDefinition];
        return requestHandler.HandleAsync(request, cancellationToken);
    }
}
