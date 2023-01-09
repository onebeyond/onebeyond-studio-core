using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.OwnedInstances;
using EnsureThat;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Domain.SharedKernel.DomainEvents;

namespace OneBeyond.Studio.Application.SharedKernel.DomainEvents;

public sealed class DIBasedPostSaveDomainEventDispatcher : IPostSaveDomainEventDispatcher
{
    private static readonly ILogger Logger = LogManager.CreateLogger<DIBasedPostSaveDomainEventDispatcher>();
    private static readonly IReadOnlyDictionary<string, object> EmptyDomainEventAmbientContext = new Dictionary<string, object>();

    private readonly ILifetimeScope _lifetimeScope;

    public DIBasedPostSaveDomainEventDispatcher(ILifetimeScope lifetimeScope)
    {
        EnsureArg.IsNotNull(lifetimeScope, nameof(lifetimeScope));

        _lifetimeScope = lifetimeScope;
    }

    async Task IPostSaveDomainEventDispatcher.DispatchAsync(
        DomainEvent domainEvent,
        IReadOnlyDictionary<string, object>? domainEventAmbientContext,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(domainEvent, nameof(domainEvent));

        var domainEventType = domainEvent.GetType();

        var handlerType = typeof(IPostSaveDomainEventHandler<>).MakeGenericType(domainEventType);

        var ownedHandlerType = typeof(Owned<>).MakeGenericType(handlerType);

        var ownedHandlers = (IEnumerable)_lifetimeScope.Resolve(
            typeof(IEnumerable<>).MakeGenericType(ownedHandlerType));

        var handlerWrapperType = typeof(PostSaveDomainEventHandler<>).MakeGenericType(domainEventType);

        var handlerWrappers = ownedHandlers
            .Cast<object>()
            .Select((handler) =>
                (PostSaveDomainEventHandler)Activator.CreateInstance(
                    handlerWrapperType, ((dynamic)handler).Value))
            .ToArray();

        if (handlerWrappers.Length == 0)
        {
            return;
        }

        var handlerExceptions = new List<Exception>();

        foreach (var handlerWrapper in handlerWrappers)
        {
            try
            {
                await handlerWrapper.HandleAsync(
                        domainEvent,
                        domainEventAmbientContext ?? EmptyDomainEventAmbientContext,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            when (!exception.IsCritical())
            {
                handlerExceptions.Add(exception);
            }
        }

        foreach (var ownedHandler in ownedHandlers)
        {
            ((dynamic)ownedHandler).Dispose();
        }

        switch (handlerExceptions.Count)
        {
            case 0:
                return;

            case 1:
                ExceptionDispatchInfo.Capture(handlerExceptions.First()).Throw();
                throw new InvalidOperationException();

            default:
                throw new AggregateException(handlerExceptions);
        }
    }

    private abstract class PostSaveDomainEventHandler
    {
        public abstract Task HandleAsync(
            DomainEvent domainEvent,
            IReadOnlyDictionary<string, object> domainEventAmbientContext,
            CancellationToken cancellationToken);
    }

    private sealed class PostSaveDomainEventHandler<TDomainEvent> : PostSaveDomainEventHandler
        where TDomainEvent : DomainEvent
    {
        private readonly IPostSaveDomainEventHandler<TDomainEvent> _handler;

        public PostSaveDomainEventHandler(IPostSaveDomainEventHandler<TDomainEvent> handler)
        {
            _handler = handler;
        }

        public override async Task HandleAsync(
            DomainEvent domainEvent,
            IReadOnlyDictionary<string, object> domainEventAmbientContext,
            CancellationToken cancellationToken)
        {
            try
            {
                await _handler.HandleAsync(
                        (TDomainEvent)domainEvent,
                        domainEventAmbientContext,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.LogError(
                    exception,
                    "Unhandled exception in {PostSaveDomainEventHandler}",
                    _handler.GetType().Name);
                throw;
            }
        }
    }
}
