using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Crosscuts.MessageQueues.DependencyInjection;
using Xunit;

namespace OneBeyond.Studio.Crosscuts.Tests;

public sealed class MessageQueueTests : TestsBase
{
    private Queue<Message>? _messageQueue;
    private Queue<Message>? _messageQueue1;
    private Queue<Message>? _messageQueue2;

    public MessageQueueTests()
        => Init();

    [Fact]
    public Task TestMessageQueueRegistrations()
    {
        var messageQueues = ServiceProvider.GetServices<IMessageQueue<Message>>();

        Assert.Equal(3, messageQueues.Count());

        var messageQueue1 = ServiceProvider.GetServices<IMessageQueue<Message, Queue1>>();

        Assert.Equal(1, messageQueue1.Count());

        var messageQueue2 = ServiceProvider.GetServices<IMessageQueue<Message, Queue2>>();

        Assert.Equal(1, messageQueue2.Count());

        return Task.CompletedTask;
    }

    protected override void ConfigureTestServices(
        IConfiguration configuration,
        IServiceCollection serviceCollection)
    {
    }

    protected override void ConfigureTestServices(
        IConfiguration configuration,
        ContainerBuilder containerBuilder)
    {
        _messageQueue = new Queue<Message>();
        _messageQueue1 = new Queue<Message>();
        _messageQueue2 = new Queue<Message>();

        containerBuilder.AddInMemoryMessageQueue(_messageQueue);
        containerBuilder.AddInMemoryMessageQueue<Message, Queue1>(_messageQueue1);
        containerBuilder.AddInMemoryMessageQueue<Message, Queue2>(_messageQueue2);
    }

    private class Message
    {
    }

    private class Queue1
    {
    }

    private class Queue2
    {
    }
}

