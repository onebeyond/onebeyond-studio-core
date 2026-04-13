using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.IntegrationEvents;
using OneBeyond.Studio.Application.SharedKernel.Tests.Infrastructure;
using OneBeyond.Studio.Application.SharedKernel.Tests.Testables;
using Xunit;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.IntegrationEvents;


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

    [Fact]
    public async Task TestIntegrationEventHandlersAreCalledWithRespectToDIScope()
    {
        var typeContainer = ServiceProvider.GetRequiredService<TestableContainer<Type>>();

        var scopedItemContainer = ServiceProvider.GetRequiredService<TestableContainer<TestableScopedItem>>();

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;

            var integrationEventDispatcher = serviceProvider.GetRequiredService<IIntegrationEventDispatcher>();

            Assert.Empty(typeContainer.Items);
            Assert.Empty(scopedItemContainer.Items);

            var integrationEvent = new TestableIntegrationEvents.ThisHappened_1_1(42, DateTimeOffset.UtcNow);

            await integrationEventDispatcher.DispatchAsync(integrationEvent, TestContext.Current.CancellationToken);

            Assert.Equal(2, typeContainer.Items.Count());
            Assert.Contains(typeof(TestableIntegrationEventHandler1), typeContainer.Items);
            Assert.Contains(typeof(TestableIntegrationEventHandler2), typeContainer.Items);

            Assert.Equal(2, scopedItemContainer.Items.Count());
            Assert.Contains(scopedItemContainer.Items, scopedItem => scopedItem.HandlerType == typeof(TestableIntegrationEventHandler1));
            Assert.Contains(scopedItemContainer.Items, scopedItem => scopedItem.HandlerType == typeof(TestableIntegrationEventHandler2));
        }
    }
}


