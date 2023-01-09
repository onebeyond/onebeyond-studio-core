using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.Application.SharedKernel.DomainEvents;

/// <summary>
/// </summary>
public sealed class DIBasedPreSaveDomainEventDispatcher : IPreSaveDomainEventDispatcher
{
    private static readonly ILogger Logger = LogManager.CreateLogger<DIBasedPreSaveDomainEventDispatcher>();

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// </summary>
    /// <param name="serviceProvider"></param>
    public DIBasedPreSaveDomainEventDispatcher(IServiceProvider serviceProvider)
    {
        EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

        _serviceProvider = serviceProvider;
    }

    async Task IPreSaveDomainEventDispatcher.DispatchAsync(
        DomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(domainEvent);

        var domainEventType = domainEvent.GetType();

        var handlerType = typeof(IPreSaveDomainEventHandler<>).MakeGenericType(domainEventType);

        var handlers = _serviceProvider.GetServices(handlerType);

        var handlerWrapperType = typeof(PreSaveDomainEventHandler<>).MakeGenericType(domainEventType);

        var handlerWrappers = handlers
            .Select(handler => (PreSaveDomainEventHandler)Activator.CreateInstance(handlerWrapperType, handler)!);

        foreach (var handlerWrapper in handlerWrappers)
        {
            await handlerWrapper.HandleAsync(
                domainEvent,
                cancellationToken).ConfigureAwait(false);
        }
    }

    private abstract class PreSaveDomainEventHandler
    {
        public abstract Task HandleAsync(DomainEvent domainEvent, CancellationToken cancellationToken);
    }

    private sealed class PreSaveDomainEventHandler<TDomainEvent> : PreSaveDomainEventHandler
        where TDomainEvent : DomainEvent
    {
        private readonly IPreSaveDomainEventHandler<TDomainEvent> _handler;

        public PreSaveDomainEventHandler(IPreSaveDomainEventHandler<TDomainEvent> handler)
            => _handler = handler;

        public override async Task HandleAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            try
            {
                await _handler.HandleAsync((TDomainEvent)domainEvent, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.LogError(
                    exception,
                    "Unhandled exception in {PreSaveDomainEventHandler}",
                    _handler.GetType().Name);
                throw;
            }
            finally
            {
                if (_handler is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
