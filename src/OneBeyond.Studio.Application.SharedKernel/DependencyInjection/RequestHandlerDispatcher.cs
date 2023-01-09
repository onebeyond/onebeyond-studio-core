using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using EnsureThat;
using MediatR;

namespace OneBeyond.Studio.Application.SharedKernel.DependencyInjection;

/// <summary>
/// Dispatches generic request handler with use of Autofac keyed registrations
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class RequestHandlerDispatcher<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IIndex<Type, IRequestHandler<TRequest, TResponse>> _requestHandlers;

    /// <summary>
    /// </summary>
    public RequestHandlerDispatcher(IIndex<Type, IRequestHandler<TRequest, TResponse>> requestHandlers)
    {
        EnsureArg.IsNotNull(requestHandlers, nameof(requestHandlers));

        _requestHandlers = requestHandlers;
    }

    private static Type RequestTypeDefinition { get; } = typeof(TRequest).GetGenericTypeDefinition();

    /// <summary>
    /// </summary>
    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var requestHandler = _requestHandlers[RequestTypeDefinition];
        return requestHandler.Handle(request, cancellationToken);
    }
}
