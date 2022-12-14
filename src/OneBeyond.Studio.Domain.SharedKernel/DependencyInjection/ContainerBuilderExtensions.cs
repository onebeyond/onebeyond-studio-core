using System.Linq;
using System.Reflection;
using Autofac;
using EnsureThat;
using MediatR;
using MoreLinq;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Domain.SharedKernel.AmbientContexts;
using OneBeyond.Studio.Domain.SharedKernel.Authorization;
using OneBeyond.Studio.Domain.SharedKernel.CommandHandlers;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Queries;
using OneBeyond.Studio.Domain.SharedKernel.IntegrationEvents;
using OneBeyond.Studio.Domain.SharedKernel.QueryHandlers;
using OneBeyond.Studio.Domain.SharedKernel.RequestAuditors;

namespace OneBeyond.Studio.Domain.SharedKernel.DependencyInjection;

/// <summary>
/// Autofac <see cref="ContainerBuilder"/> extensions
/// </summary>
public static class ContainerBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="IRequestHandler{TRequest, TResponse}"/> for CRUD operations
    /// </summary>
    /// <param name="containerBuilder"></param>
    /// <param name="requestHandlersAssemblies"></param>
    public static ContainerBuilder AddMediatorRequestHandlers(
        this ContainerBuilder containerBuilder,
        params Assembly[] requestHandlersAssemblies)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder.RegisterGeneric(typeof(CreateHandler<,,>))
            .Keyed(typeof(Create<,,>), typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();
        containerBuilder.RegisterGeneric(typeof(UpdateHandler<,,>))
            .Keyed(typeof(Update<,,>), typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();
        containerBuilder.RegisterGeneric(typeof(DeleteHandler<,>))
            .Keyed(typeof(Delete<,>), typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();
        containerBuilder.RegisterGeneric(typeof(GetByIdHandler<,,>))
            .Keyed(typeof(GetById<,,>), typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();
        containerBuilder.RegisterGeneric(typeof(ListHandler<,,>))
            .Keyed(typeof(List<,,>), typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();
        containerBuilder.RegisterAssemblyTypes(requestHandlersAssemblies)
            .AsClosedTypesOf(typeof(IRequestHandler<,>))
            .InstancePerLifetimeScope();
        containerBuilder.RegisterGeneric(typeof(RequestHandlerDispatcher<,>))
            .As(typeof(IRequestHandler<,>));

        containerBuilder.RegisterGeneric(typeof(RequestAuditor<,>))
            .As(typeof(IPipelineBehavior<,>))
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
    public static ContainerBuilder AddIntegratonEvents(
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
            .As(typeof(IPipelineBehavior<,>))
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
                    && typeof(IBaseRequest).IsAssignableFrom(type.GetGenericArguments()[0])
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
