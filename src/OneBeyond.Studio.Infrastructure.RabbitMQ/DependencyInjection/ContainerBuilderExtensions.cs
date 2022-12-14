using Autofac;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.RabbitMQ.Options;

namespace OneBeyond.Studio.Infrastructure.RabbitMQ.DependencyInjection;

public static class ContainerBuilderExtensions
{
    public static ContainerBuilder AddRabbitPubSubMessageQueue<TMessage>(
        this ContainerBuilder containerBuilder,
        RabbitPubSubMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        containerBuilder.Register(
                (componentContext) => new RabbitPubSubMessageQueue<TMessage>(options))
            .As<IMessageQueue<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddRabbitPubSubMessageQueue<TMessage, TDiscriminator>(
        this ContainerBuilder containerBuilder,
        RabbitPubSubMessageQueueOptions options,
        bool isDefault = false)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        var registration = containerBuilder.Register(
                (componentContext) => new RabbitPubSubMessageQueue<TMessage, TDiscriminator>(options))
            .As<IMessageQueue<TMessage, TDiscriminator>>()
            .SingleInstance();

        if (isDefault)
        {
            registration.As<IMessageQueue<TMessage>>();
        }

        return containerBuilder;
    }

    public static ContainerBuilder AddRabbitPubSubMessageQueueReceiver<TMessage>(
        this ContainerBuilder containerBuilder,
        RabbitPubSubMessageQueueReceiverOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder.Register(
                (componentContext) => new RabbitPubSubMessageQueueReceiver<TMessage>(options))
            .As<IMessageQueueReceiver<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddRabbitMessageQueue<TMessage>(
        this ContainerBuilder containerBuilder,
        RabbitMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        containerBuilder.Register(
                (componentContext) => new RabbitMessageQueue<TMessage>(options))
            .As<IMessageQueue<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddRabbitMessageQueue<TMessage, TDiscriminator>(
        this ContainerBuilder containerBuilder,
        RabbitMessageQueueOptions options,
        bool isDefault = false)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        var registration = containerBuilder.Register(
                (componentContext) => new RabbitMessageQueue<TMessage, TDiscriminator>(options))
            .As<IMessageQueue<TMessage, TDiscriminator>>()
            .SingleInstance();

        if (isDefault)
        {
            registration.As<IMessageQueue<TMessage>>();
        }

        return containerBuilder;
    }

    public static ContainerBuilder AddRabbitMessageQueueReceiver<TMessage>(
        this ContainerBuilder containerBuilder,
        RabbitMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        containerBuilder.Register(
                (componentContext) => new RabbitMessageQueueReceiver<TMessage>(options))
            .As<IMessageQueueReceiver<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }
}
