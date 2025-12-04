using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using EnsureThat;
using OneBeyond.Studio.Core.Mediator.Commands;

namespace OneBeyond.Studio.Application.SharedKernel.DependencyInjection;

/// <summary>
/// Dispatches generic command handlers with use of Autofac keyed registrations
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class CommandHandlerDispatcher<TRequest, TResponse> : ICommandHandler<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IIndex<Type, ICommandHandler<TRequest, TResponse>> _requestHandlers;

    /// <summary>
    /// </summary>
    public CommandHandlerDispatcher(IIndex<Type, ICommandHandler<TRequest, TResponse>> requestHandlers)
    {
        EnsureArg.IsNotNull(requestHandlers, nameof(requestHandlers));

        _requestHandlers = requestHandlers;
    }

    private static Type RequestTypeDefinition { get; } = typeof(TRequest).GetGenericTypeDefinition();

    /// <summary>
    /// </summary>
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var requestHandler = _requestHandlers[RequestTypeDefinition];
        return requestHandler.HandleAsync(request, cancellationToken);
    }
}
