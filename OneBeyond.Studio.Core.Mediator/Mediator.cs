using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Core.Mediator.Commands;
using OneBeyond.Studio.Core.Mediator.Notifications;
using OneBeyond.Studio.Core.Mediator.Pipelines;
using OneBeyond.Studio.Core.Mediator.Queries;

namespace OneBeyond.Studio.Core.Mediator;

public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));
    }

    public async Task CommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : class, ICommand
    {
        EnsureArg.IsNotNull(command, nameof(command));

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();

        // Low risk - as handlers should be DIed by assembly scan - namely just to catch mistakes.
        if (handler is null)
        {
            throw new InvalidOperationException($"A handler needs to be registered for Command {typeof(TCommand)}");
        }

        var pipeline = _serviceProvider.GetServices<IMediatorPipelineBehaviour<TCommand>>();
        var handlerDelegate = () => handler.HandleAsync(command, cancellationToken);

        foreach (var behaviour in pipeline)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behaviour.HandleAsync(command, next, cancellationToken);
        }

        await handlerDelegate();
    }

    public async Task<TResult> CommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : class, ICommand<TResult>
    {
        EnsureArg.IsNotNull(command, nameof(command));

        var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();

        // Low risk - as handlers should be DIed by assembly scan - namely just to catch mistakes.
        if (handler is null)
        {
            throw new InvalidOperationException($"A handler needs to be registered for Command {typeof(TCommand)}");
        }

        var pipeline = _serviceProvider.GetServices<IMediatorPipelineBehaviour<TCommand, TResult>>();
        var handlerDelegate = () => handler.HandleAsync(command, cancellationToken);

        foreach (var behaviour in pipeline)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behaviour.HandleAsync(command, next, cancellationToken);
        }

        return await handlerDelegate();
        
    }

    public async Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification: class, INotification
    {
        EnsureArg.IsNotNull(notification);

        var receivers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();

        var delegateList = receivers.Select(r => r.HandleAsync(notification, cancellationToken));

        await Task.WhenAll(delegateList);
    }

    public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : class, IQuery<TResult>
    {
        EnsureArg.IsNotNull(query, nameof(query));

        var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();
        if (handler == null)
        {
            throw new InvalidOperationException($"A handler needs to be registered for Query {typeof(TQuery)}");
        }

        var behaviors = _serviceProvider.GetServices<IMediatorPipelineBehaviour<TQuery, TResult>>().Reverse();
        Func<Task<TResult>> handlerDelegate = () => handler.HandleAsync(query, cancellationToken);
        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.HandleAsync(query, next, cancellationToken);
        }

        return await handlerDelegate();
    }
}
