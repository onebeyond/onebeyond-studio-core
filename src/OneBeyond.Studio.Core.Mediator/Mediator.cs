using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Core.Mediator.Notifications;
using OneBeyond.Studio.Core.Mediator.Pipelines;

namespace OneBeyond.Studio.Core.Mediator;

public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));
    }

    /// <inheritdoc/>
    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, IRequest
    {
        EnsureArg.IsNotNull(request, nameof(request));

        var handler = _serviceProvider.GetService<IRequestHandler<TRequest>>();
        
        // Low risk - as handlers should be DIed by assembly scan - namely just to catch mistakes.
        if (handler is null)
        {
            throw new InvalidOperationException($"A handler needs to be registered for request {typeof(TRequest)}");
        }

        var pipeline = _serviceProvider.GetServices<IMediatorPipelineBehaviour<TRequest>>();
        var handlerDelegate = () => handler.Handle(request, cancellationToken);

        foreach (var behaviour in pipeline)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behaviour.HandleAsync(request, next, cancellationToken);
        }

        await handlerDelegate();
    }

    /// <inheritdoc/>
    public async Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNull(request, nameof(request));

        var handler = _serviceProvider.GetService<IRequestHandler<IRequest<TResult>, TResult>>();

        // Low risk - as handlers should be DIed by assembly scan - namely just to catch mistakes.
        if (handler is null)
        {
            throw new InvalidOperationException($"A handler needs to be registered for request {typeof(IRequest<TResult>)}");
        }

        var pipeline = _serviceProvider.GetServices<IMediatorPipelineBehaviour<IRequest<TResult>, TResult>>();
        var handlerDelegate = () => handler.Handle(request, cancellationToken);

        foreach (var behaviour in pipeline)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behaviour.HandleAsync(request, next, cancellationToken);
        }

        return await handlerDelegate();
        
    }

    /// <inheritdoc/>
    public async Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) 
        where TNotification: class, INotification
    {
        EnsureArg.IsNotNull(notification);

        var receivers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();

        var delegateList = receivers.Select(r => r.HandleAsync(notification, cancellationToken));

        foreach (var action in delegateList)
        {
            await action;
        }
    }
}
