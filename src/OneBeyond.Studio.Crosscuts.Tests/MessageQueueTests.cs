using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Crosscuts.MessageQueues;
using OneBeyond.Studio.Crosscuts.MessageQueues.DependencyInjection;

namespace OneBeyond.Studio.Crosscuts.Tests;

[TestClass]
public sealed class MessageQueueTests : TestsBase
{
    private readonly Queue<Message> _messageQueue;
    private readonly Queue<Message> _messageQueue1;
    private readonly Queue<Message> _messageQueue2;

    public MessageQueueTests()
    {
        _messageQueue = new Queue<Message>();
        _messageQueue1 = new Queue<Message>();
        _messageQueue2 = new Queue<Message>();
    }

    [TestMethod]
    public Task TestMessageQueueRegistrations()
    {
        var messageQueues = ServiceProvider.GetServices<IMessageQueue<Message>>();

        Assert.AreEqual(3, messageQueues.Count());

        var messageQueue1 = ServiceProvider.GetServices<IMessageQueue<Message, Queue1>>();

        Assert.AreEqual(1, messageQueue1.Count());

        var messageQueue2 = ServiceProvider.GetServices<IMessageQueue<Message, Queue2>>();

        Assert.AreEqual(1, messageQueue2.Count());

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
