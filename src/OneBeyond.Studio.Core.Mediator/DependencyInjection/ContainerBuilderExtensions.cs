using System.Reflection;
using Autofac;
using EnsureThat;
using OneBeyond.Studio.Core.Mediator.Commands;
using OneBeyond.Studio.Core.Mediator.Notifications;
using OneBeyond.Studio.Core.Mediator.Pipelines;
using OneBeyond.Studio.Core.Mediator.Queries;

namespace OneBeyond.Studio.Core.Mediator.DependencyInjection;

public static class ContainerBuilderExtensions
{
    /// <summary>
    /// Registers all Mediator request handlers found in a given Autofac container
    /// </summary>
    /// <param name="containerBuilder"></param>
    /// <param name="assemblies">Parameter list of assemblies to scan for mediator handlers</param>
    /// <returns></returns>
    public static ContainerBuilder AddMediatorHandlers(
        this ContainerBuilder containerBuilder,
        params Assembly[] assemblies)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder
            .RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(ICommandHandler<,>))
            .InstancePerLifetimeScope();

        containerBuilder
            .RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(ICommandHandler<>))
            .InstancePerLifetimeScope();

        containerBuilder
            .RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(IQueryHandler<,>))
            .InstancePerLifetimeScope();

        containerBuilder
            .RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(INotificationHandler<>))
            .InstancePerLifetimeScope();

        containerBuilder
            .RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(IMediatorPipelineBehaviour<,>))
            .InstancePerLifetimeScope();

        return containerBuilder;
    }
}
