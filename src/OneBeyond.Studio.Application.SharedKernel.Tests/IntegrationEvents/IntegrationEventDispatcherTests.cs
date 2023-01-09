using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Application.SharedKernel.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;
using OneBeyond.Studio.Application.SharedKernel.Tests.Infrastructure;
using OneBeyond.Studio.Application.SharedKernel.Tests.Testables;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.IntegrationEvents;

[TestClass]
public sealed class IntegrationEventDispatcherTests : TestsBase
{
    protected override void ConfigureTestServices(IConfiguration configuration, IServiceCollection serviceCollection)
    {
    }

    protected override void ConfigureTestServices(IConfiguration configuration, ContainerBuilder containerBuilder)
    {
        containerBuilder.AddIntegrationEvents();

        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AsClosedTypesOf(typeof(IIntegrationEventHandler<>))
            .InstancePerLifetimeScope();

        containerBuilder.RegisterType<TestableScopedItem>()
            .AsSelf()
            .InstancePerLifetimeScope();

        containerBuilder.RegisterGeneric(typeof(TestableContainer<>))
            .AsSelf()
            .SingleInstance();
    }

    [TestMethod]
    public async Task TestIntegrationEventHandlersAreCalledWithRespectToDIScope()
    {
        var typeContainer = ServiceProvider.GetRequiredService<TestableContainer<Type>>();

        var scopedItemContainer = ServiceProvider.GetRequiredService<TestableContainer<TestableScopedItem>>();

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;

            var integrationEventDispatcher = serviceProvider.GetRequiredService<IIntegrationEventDispatcher>();

            Assert.AreEqual(0, typeContainer.Items.Count);
            Assert.AreEqual(0, scopedItemContainer.Items.Count);

            var integrationEvent = new TestableIntegrationEvents.ThisHappened_1_1(42, DateTimeOffset.UtcNow);

            await integrationEventDispatcher.DispatchAsync(integrationEvent);

            Assert.AreEqual(2, typeContainer.Items.Count);
            Assert.IsTrue(typeContainer.Items.Contains(typeof(TestableIntegrationEventHandler1)));
            Assert.IsTrue(typeContainer.Items.Contains(typeof(TestableIntegrationEventHandler2)));

            Assert.AreEqual(2, scopedItemContainer.Items.Count);
            Assert.IsTrue(scopedItemContainer.Items.Any((scopedItem) => scopedItem.HandlerType == typeof(TestableIntegrationEventHandler1)));
            Assert.IsTrue(scopedItemContainer.Items.Any((scopedItem) => scopedItem.HandlerType == typeof(TestableIntegrationEventHandler2)));
        }
    }
}
