using System.Collections.Generic;
using Autofac;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.MessageQueues.InMemory;

namespace OneBeyond.Studio.Crosscuts.MessageQueues.DependencyInjection;

/// <summary>
/// </summary>
public static class ContainerBuilderExtensions
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TDiscriminator"></typeparam>
    /// <param name="containerBuilder"></param>
    /// <param name="queue"></param>
    /// <returns></returns>
    public static ContainerBuilder AddInMemoryMessageQueue<TMessage, TDiscriminator>(
        this ContainerBuilder containerBuilder,
        Queue<TMessage> queue)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder.Register(
                (componentContext) =>
                    new InMemoryMessageQueue<TMessage, TDiscriminator>(queue))
            .As<IMessageQueue<TMessage, TDiscriminator>>()
            .As<IMessageQueue<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="containerBuilder"></param>
    /// <param name="queue"></param>
    /// <returns></returns>
    public static ContainerBuilder AddInMemoryMessageQueue<TMessage>(
        this ContainerBuilder containerBuilder,
        Queue<TMessage> queue)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder.Register(
                (componentContext) =>
                    new InMemoryMessageQueue<TMessage>(queue))
            .As<IMessageQueue<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }
}
