using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.OwnedInstances;
using EnsureThat;
using Microsoft.Extensions.Logging;
using OneBeyond.Studio.Crosscuts.Exceptions;
using OneBeyond.Studio.Crosscuts.Logging;
using OneBeyond.Studio.Crosscuts.Reflection;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;

namespace OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;

/// <summary>
/// </summary>
public sealed class DIBasedIntegrationEventDispatcher : IIntegrationEventDispatcher
{
    private static readonly ILogger Logger = LogManager.CreateLogger<DIBasedIntegrationEventDispatcher>();
    private static readonly MethodInfo CreateHandlerWrapperMethod =
        Reflector.MethodFrom(() => CreateHandlerWrapper<IntegrationEvent>(default!)).GetGenericMethodDefinition();
    private static readonly ConcurrentDictionary<Type, Func<object, IntegrationEventHandler>> IntegrationEventHandlerCtors =
        new ConcurrentDictionary<Type, Func<object, IntegrationEventHandler>>();

    private readonly ILifetimeScope _lifetimeScope;

    /// <summary>
    /// </summary>
    /// <param name="lifetimeScope"></param>
    public DIBasedIntegrationEventDispatcher(ILifetimeScope lifetimeScope)
    {
        EnsureArg.IsNotNull(lifetimeScope, nameof(lifetimeScope));

        _lifetimeScope = lifetimeScope;
    }

    async Task IIntegrationEventDispatcher.DispatchAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(integrationEvent, nameof(integrationEvent));

        var integrationEventType = integrationEvent.GetType();

        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEventType);

        var ownedHandlerType = typeof(Owned<>).MakeGenericType(handlerType);

        var ownedHandlers = (IEnumerable)_lifetimeScope.Resolve(
            typeof(IEnumerable<>).MakeGenericType(ownedHandlerType));

        var handlerWrappers = ownedHandlers
            .Cast<object>()
            .Select(
                (handler) => CreateHandlerWrapper(integrationEventType, handler))
            .ToArray();

        if (handlerWrappers.Length == 0)
        {
            Logger.LogInformation(
                "There are no handlers found for processing integration event of {IntegrationEventClrType} CLR type",
                integrationEventType.AssemblyQualifiedName);
            return;
        }

        var handlerExceptions = new List<Exception>();

        // TODO: Think about running them in parallel. It should no bring any problems as they are
        //       executed in independent DI scopes.
        foreach (var handlerWrapper in handlerWrappers)
        {
            try
            {
                await handlerWrapper.HandleAsync(
                    integrationEvent,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            when (!exception.IsCritical())
            {
                handlerExceptions.Add(exception);
            }
        }

        foreach (var ownedHandler in ownedHandlers)
        {
            ((IDisposable)ownedHandler).Dispose();
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

    private abstract class IntegrationEventHandler
    {
        public abstract Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
    }

    private sealed class IntegrationEventHandler<TIntegrationEvent> : IntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        private readonly IIntegrationEventHandler<TIntegrationEvent> _handler;

        public IntegrationEventHandler(IIntegrationEventHandler<TIntegrationEvent> handler)
        {
            _handler = handler;
        }

        public override async Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            try
            {
                await _handler.HandleAsync((TIntegrationEvent)integrationEvent, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.LogError(
                    exception,
                    "Unhandled exception in {IntegrationEventHandler}",
                    _handler.GetType().Name);
                throw;
            }
        }
    }

    private static IntegrationEventHandler CreateHandlerWrapper(Type integrationEventType, object ownedHandler)
    {
        var integrationEventHandlerCtor = IntegrationEventHandlerCtors.GetOrAdd(
                        integrationEventType,
                        (_) =>
                        {
                            var parameterExpression = Expression.Parameter(typeof(object), "ownedHandler");
                            var callExpression = Expression.Call(
                                instance: null,
                                CreateHandlerWrapperMethod.MakeGenericMethod(integrationEventType), parameterExpression);
                            var lambdaExpression = Expression.Lambda<Func<object, IntegrationEventHandler>>(
                                callExpression,
                                parameterExpression);
                            return lambdaExpression.Compile();
                        });
        return integrationEventHandlerCtor(ownedHandler);
    }

    private static IntegrationEventHandler CreateHandlerWrapper<TIntegrationEvent>(object ownedHandler)
        where TIntegrationEvent : IntegrationEvent
    {
        var typedOwnedHandler = (Owned<IIntegrationEventHandler<TIntegrationEvent>>)ownedHandler;
        return new IntegrationEventHandler<TIntegrationEvent>(typedOwnedHandler.Value);
    }
}
