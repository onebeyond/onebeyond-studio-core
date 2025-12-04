using System.Linq;
using System.Reflection;
using Autofac;
using EnsureThat;
using MoreLinq;
using OneBeyond.Studio.Application.SharedKernel.AmbientContexts;
using OneBeyond.Studio.Application.SharedKernel.Authorization;
using OneBeyond.Studio.Application.SharedKernel.CommandHandlers;
using OneBeyond.Studio.Application.SharedKernel.Entities.Queries;
using OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;
using OneBeyond.Studio.Application.SharedKernel.QueryHandlers;
using OneBeyond.Studio.Application.SharedKernel.RequestAuditors;
using OneBeyond.Studio.Core.Mediator.Commands;
using OneBeyond.Studio.Core.Mediator.Pipelines;
using OneBeyond.Studio.Core.Mediator.Queries;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

namespace OneBeyond.Studio.Application.SharedKernel.DependencyInjection;

/// <summary>
/// Autofac <see cref="ContainerBuilder"/> extensions
/// </summary>
public static class ContainerBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="ICommandHandler{TRequest, TResponse}"/> for CRUD operations <br/>
    /// Registers <see cref="IQueryHandler{TRequest, TResponse}"/> for CRUD operations
    /// </summary>
    /// <param name="containerBuilder"></param>
    /// <param name="requestHandlersAssemblies"></param>
    public static ContainerBuilder AddMediatorRequestHandlers(
        this ContainerBuilder containerBuilder,
        params Assembly[] requestHandlersAssemblies)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        
        containerBuilder.RegisterGeneric(typeof(DeleteHandler<,>))
            .Keyed(typeof(Delete<,>), typeof(ICommandHandler<,>))
            .InstancePerLifetimeScope();
        containerBuilder.RegisterGeneric(typeof(GetByIdHandler<,,>))
            .Keyed(typeof(GetById<,,>), typeof(IQueryHandler<,>))
            .InstancePerLifetimeScope();
        containerBuilder.RegisterGeneric(typeof(ListHandler<,,>))
            .Keyed(typeof(List<,,>), typeof(IQueryHandler<,>))
            .InstancePerLifetimeScope();
        
        containerBuilder.AddMediatorRequestHandlers(requestHandlersAssemblies);

        containerBuilder.RegisterGeneric(typeof(CommandHandlerDispatcher<,>))
            .As(typeof(ICommandHandler<,>));

        containerBuilder.RegisterGeneric(typeof(QueryHandlerDispatcher<,>))
            .As(typeof(IQueryHandler<,>));

        containerBuilder.RegisterGeneric(typeof(RequestAuditor<,>))
            .As(typeof(IMediatorPipelineBehaviour<,>))
            .InstancePerLifetimeScope();
        containerBuilder.RegisterGeneric(typeof(RequestAuditRecordBuilder<>))
            .As(typeof(IRequestAuditRecordBuilder<>))
            .InstancePerDependency();

        return containerBuilder;
    }

    /// <summary>
    /// </summary>
    /// <param name="containerBuilder"></param>
    /// <param name="integrationEventsAssemblies"></param>
    /// <returns></returns>
    public static ContainerBuilder AddIntegrationEvents(
        this ContainerBuilder containerBuilder,
        params Assembly[] integrationEventsAssemblies)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder.RegisterType<MQBasedIntegrationEventPublisher>()
            .As<IIntegrationEventPublisher>()
            .SingleInstance();

        containerBuilder.Register(
                (componentContext) =>
                {
                    var integrationEventQueue =
                        componentContext.Resolve<IMessageQueueReceiver<IntegrationEventEnvelope>>();
                    return new MQBasedIntegrationEventReceiver(
                        integrationEventQueue,
                        integrationEventsAssemblies);
                })
            .As<IIntegrationEventReceiver>()
            .SingleInstance();

        containerBuilder.RegisterType<DIBasedIntegrationEventDispatcher>()
            .As<IIntegrationEventDispatcher>()
            .InstancePerDependency();

        return containerBuilder;
    }

    /// <summary>
    /// Registers authorization requirement handlers implemented in specified assemblies.
    /// </summary>
    public static ContainerBuilder AddAuthorizationRequirementHandlers(
        this ContainerBuilder containerBuilder,
        AuthorizationOptions authorizationOptions,
        params Assembly[] handlerAssemblies)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        EnsureArg.IsNotNull(authorizationOptions, nameof(authorizationOptions));
        EnsureArg.IsNotNull(handlerAssemblies, nameof(handlerAssemblies));

        containerBuilder.RegisterInstance(authorizationOptions)
            .SingleInstance();

        containerBuilder.RegisterGeneric(typeof(AuthorizationRequirementBehavior<,>))
            .As(typeof(IMediatorPipelineBehaviour<,>))
            .InstancePerLifetimeScope();

        // NB: It may turn out this code needs further tweaking. At the moment it covers
        //     a case when there is open generic requirement handler with a single parameter for
        //     request and it explicitely implements IAuthorizationRequirementHandler<FixedRequirement,>
        handlerAssemblies
            .SelectMany(
                (assembly) => assembly.GetTypes())
            .Where(
                (type) =>
                    !type.IsAbstract
                    && type.IsGenericTypeDefinition
                    && type.GetGenericArguments().Length == 1
                    && (typeof(ICommand<>).IsAssignableFrom(type.GetGenericArguments()[0]) 
                        || typeof(IQuery<>).IsAssignableFrom(type.GetGenericArguments()[0]))
                    && type.GetInterfaces().Any(
                        (iface) => iface.IsGenericType
                            && iface.GetGenericTypeDefinition() == typeof(IAuthorizationRequirementHandler<,>)))
            .ForEach(
                (handlerOpenGenericType) =>
                {
                    containerBuilder.RegisterGeneric(handlerOpenGenericType)
                        .As(typeof(IAuthorizationRequirementHandler<,>))
                        .InstancePerLifetimeScope();
                });

        containerBuilder.RegisterAssemblyTypes(handlerAssemblies)
            .AsClosedTypesOf(typeof(IAuthorizationRequirementHandler<,>))
            .InstancePerLifetimeScope();

        return containerBuilder;
    }

    /// <summary>
    /// Registers ambient context accessor optionally wrapped into overrider allowing
    /// to redefine an ambient state at runtime.
    /// </summary>
    public static ContainerBuilder AddAmbientContextAccessor<TAmbientStateAccessor, TAmbientState>(
        this ContainerBuilder containerBuilder,
        bool withOverrider = false)
        where TAmbientState : AmbientContext
        where TAmbientStateAccessor : IAmbientContextAccessor<TAmbientState>
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder.RegisterType<TAmbientStateAccessor>()
            .As<IAmbientContextAccessor>()
            .As<IAmbientContextAccessor<TAmbientState>>()
            .InstancePerLifetimeScope();

        if (withOverrider)
        {
            containerBuilder.RegisterDecorator<IAmbientContextAccessor>(
                (_, _, ambientStateAccessor) => new AmbientContextAccessorOverrider<TAmbientState>(ambientStateAccessor));
            containerBuilder.RegisterDecorator<IAmbientContextAccessor<TAmbientState>>(
                (_, _, ambientStateAccessor) => new AmbientContextAccessorOverrider<TAmbientState>(ambientStateAccessor));
        }

        return containerBuilder;
    }
}
