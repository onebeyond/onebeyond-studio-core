using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Crosscuts.Strings;
using OneBeyond.Studio.Infrastructure.Azure.Exceptions;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues;
using OneBeyond.Studio.Infrastructure.Azure.MessageQueues.Options;

namespace OneBeyond.Studio.Infrastructure.Azure.Tests.MessageQueues;

[TestClass]
public sealed class ServiceBusMessageQueueTests : TestsBase
{
    private readonly string _connectionString;

    public ServiceBusMessageQueueTests()
        : base()
    {
        _connectionString = ""; // Please provide your own connection string
    }

    [TestMethod]
    public async Task Publishing_Sessioned_Message()
    {
        if (_connectionString.IsNullOrEmpty())
        {
            return;
        }
        var queueId = Guid.NewGuid();
        await using (var messageQueue = GetMessageQueueWithSessions(queueId))
        {
            var sessionId = Guid.NewGuid();
            var message = new Message(sessionId, "Hello World");

            await messageQueue.PublishAsync(message, CancellationToken.None);
            Assert.IsTrue(true, "For shutting up Sonar Cloud");
        }
    }

    [TestMethod]
    public async Task Publishing_Ordinary_Message()
    {
        if (_connectionString.IsNullOrEmpty())
        {
            return;
        }
        var queueId = Guid.NewGuid();
        await using (var messageQueue = GetMessageQueueWithoutSessions(queueId))
        {
            var sessionId = Guid.NewGuid();
            var message = new Message(sessionId, "Hello World");

            await messageQueue.PublishAsync(message, CancellationToken.None);
            Assert.IsTrue(true, "For shutting up Sonar Cloud");
        }
    }

    [TestMethod]
    public async Task Throwing_When_Existing_Queue_With_Sessions_Accessed_Without_SessionId_In_Options()
    {
        if (_connectionString.IsNullOrEmpty())
        {
            return;
        }
        var queueId = Guid.NewGuid();
        await using (var messageQueue = GetMessageQueueWithSessions(queueId))
        {
            var sessionId = Guid.NewGuid();
            var message = new Message(sessionId, "Hello World");

            await messageQueue.PublishAsync(message, CancellationToken.None);
        }
        await using (var messageQueue = GetMessageQueueWithoutSessions(queueId))
        {
            var sessionId = Guid.NewGuid();
            var message = new Message(sessionId, "Hello World");

            try
            {
                await messageQueue.PublishAsync(message, CancellationToken.None);
                Assert.Fail();
            }
            catch (AzureInfrastructureException)
            {
            }
        }
    }

    protected override void ConfigureTestServices(IConfiguration configuration, IServiceCollection serviceCollection)
    {
    }

    protected override void ConfigureTestServices(IConfiguration configuration, ContainerBuilder containerBuilder)
    {
    }

    private ServiceBusMessageQueue<Message, Discriminator> GetMessageQueueWithoutSessions(Guid queueId)
    {
        var options = new AzureServiceBusMessageQueueOptions
        {
            ConnectionString = _connectionString,
            QueueName = $"sbq-{queueId}"
        };
        return new ServiceBusMessageQueue<Message, Discriminator>(options);
    }

    private ServiceBusMessageQueue<Message, Discriminator> GetMessageQueueWithSessions(Guid queueId)
    {
        var options = new AzureServiceBusMessageQueueOptions
        {
            ConnectionString = _connectionString,
            QueueName = $"sbq-{queueId}",
            SessionId = nameof(Message.SessionId).ToLower()
        };
        return new ServiceBusMessageQueue<Message, Discriminator>(options);
    }

    private sealed record Message(
        Guid SessionId,
        string Payload);

    private sealed record Discriminator();
}
