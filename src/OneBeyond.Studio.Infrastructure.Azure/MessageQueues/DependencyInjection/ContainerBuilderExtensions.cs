using Autofac;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

namespace OneBeyond.Studio.Infrastructure.Azure.MessageQueues.DependencyInjection;

public static class ContainerBuilderExtensions
{
    public static ContainerBuilder AddAzureLargeMessageQueue<TMessage>(
        this ContainerBuilder containerBuilder,
        AzureLargeMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder.Register(
                (componentContext) =>
                    new StorageLargeMessageQueue<TMessage>(options))
            .As<IMessageQueue<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddAzureLargeMessageQueue<TMessage, TDiscriminator>(
        this ContainerBuilder containerBuilder,
        AzureLargeMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder.Register(
                (componentContext) =>
                    new AzureLargeMessageQueue<TMessage, TDiscriminator>(options))
            .As<IMessageQueue<TMessage, TDiscriminator>>()
            .As<IMessageQueue<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddAzureMessageQueue<TMessage>(
        this ContainerBuilder containerBuilder,
        AzureMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        containerBuilder.Register(
                (componentContext) =>
                    new StorageMessageQueue<TMessage>(options))
            .As<IMessageQueue<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddAzureMessageQueue<TMessage, TDiscriminator>(
        this ContainerBuilder containerBuilder,
        AzureMessageQueueOptions options)
        where TMessage : class
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder.Register(
                (componentContext) =>
                    new AzureMessageQueue<TMessage, TDiscriminator>(options))
            .As<IMessageQueue<TMessage, TDiscriminator>>()
            .As<IMessageQueue<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddAzureServiceBusMessageQueue<TMessage>(
        this ContainerBuilder containerBuilder,
        AzureServiceBusMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        containerBuilder.Register(
                (componentContext) =>
                    new ServiceBusMessageQueue<TMessage>(options))
            .As<IMessageQueue<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddAzureServiceBusMessageQueue<TMessage, TDiscriminator>(
        this ContainerBuilder containerBuilder,
        AzureServiceBusMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        containerBuilder.Register(
                (componentContext) =>
                    new ServiceBusMessageQueue<TMessage, TDiscriminator>(options))
            .As<IMessageQueue<TMessage, TDiscriminator>>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddAzureServicePubSubBusMessageQueue<TMessage>(
        this ContainerBuilder containerBuilder,
        AzureServicePubSubBusMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));
        EnsureArg.IsNotNull(options, nameof(options));

        containerBuilder.Register(
                (componentContext) =>
                    new ServiceBusPubSubMessageQueue<TMessage>(options))
            .As<IMessageQueue<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddServiceBusMessageQueueReceiver<TMessage>(
        this ContainerBuilder containerBuilder,
        AzureMessageQueueOptions options)
    {
        EnsureArg.IsNotNull(containerBuilder, nameof(containerBuilder));

        containerBuilder.Register(
                (componentContext) => new ServiceBusMessageQueueReceiver<TMessage>(options))
            .As<IMessageQueueReceiver<TMessage>>()
            .SingleInstance();

        return containerBuilder;
    }
}
