using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Application.SharedKernel.DomainEvents;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Application.SharedKernel.Specifications;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.PurchaseOrders;
using OneBeyond.Studio.DataAccess.EFCore.Tests.Entities.Services;

namespace OneBeyond.Studio.DataAccess.EFCore.Tests;

[TestClass]
public sealed class DomainEventsTests : InMemoryTestsBase
{
    public DomainEventsTests()
        : base(withDomainEvents: true)
    {
    }

    protected override void ConfigureTestServices(
        IConfiguration configuration,
        ContainerBuilder containerBuilder)
    {
        base.ConfigureTestServices(configuration, containerBuilder);

        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AsClosedTypesOf(typeof(IPreSaveDomainEventHandler<>))
            .InstancePerLifetimeScope();

        containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AsClosedTypesOf(typeof(IPostSaveDomainEventHandler<>))
            .InstancePerLifetimeScope();

        containerBuilder.RegisterGeneric(typeof(Container<>))
            .AsSelf()
            .SingleInstance();

        containerBuilder.RegisterType<PostSaveScopedItem>()
            .AsSelf()
            .InstancePerLifetimeScope();

        containerBuilder.RegisterType<PreSaveScopedItem>()
            .AsSelf()
            .InstancePerLifetimeScope();
    }

    [TestMethod]
    public async Task TestDomainEventHandlersAreCalledAndDomainEventsAreQueued()
    {
        var purchaseOrderId = default(Guid);

        var preSaveScopedItemContainer = ServiceProvider.GetRequiredService<Container<PreSaveScopedItem>>();

        var postSaveScopedItemContainer = ServiceProvider.GetRequiredService<Container<PostSaveScopedItem>>();

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var purchaseOrderRWRepository =
                serviceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = new PurchaseOrder();
            purchaseOrder.AddLine("First");
            purchaseOrder.AddLine("Second");

            preSaveScopedItemContainer.Clear();
            postSaveScopedItemContainer.Clear();

            await purchaseOrderRWRepository.CreateAsync(purchaseOrder, default);

            purchaseOrderId = purchaseOrder.Id;

            var dbContext = serviceProvider.GetRequiredService<DbContexts.DbContext>();
            var postSaveDomainEventDispatcher = serviceProvider.GetRequiredService<IPostSaveDomainEventDispatcher>();
            var raisedDomainEventReceiver = serviceProvider.GetRequiredService<IRaisedDomainEventReceiver>();
            var raisedDomainEventCount = await dbContext.Set<RaisedDomainEvent>().CountAsync();

            Assert.AreEqual(2, raisedDomainEventCount);
            Assert.AreEqual(1, preSaveScopedItemContainer.Items.Count);
            Assert.AreEqual(0, postSaveScopedItemContainer.Items.Count);

            var cancellationTokenSource = new CancellationTokenSource();

            await raisedDomainEventReceiver.RunAsync(
                async (raisedDomainEvent, cancellationToken) =>
                {
                    await postSaveDomainEventDispatcher.DispatchAsync(
                        raisedDomainEvent.GetValue(),
                        raisedDomainEvent.GetAmbientContext(),
                        cancellationToken);
                    --raisedDomainEventCount;
                    if (raisedDomainEventCount == 0)
                    {
                        cancellationTokenSource.Cancel();
                    }
                },
                cancellationTokenSource.Token);

            Assert.AreEqual(4, postSaveScopedItemContainer.Items.Count);
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var purchaseOrderRWRepository =
                serviceProvider.GetRequiredService<IRWRepository<PurchaseOrder, Guid>>();

            var purchaseOrder = await purchaseOrderRWRepository.GetByIdAsync(
                purchaseOrderId,
                Includes.Create(
                    (PurchaseOrder purchaseOrder) => purchaseOrder.Lines),
                default);

            purchaseOrder.AddLine("Third");

            preSaveScopedItemContainer.Clear();
            postSaveScopedItemContainer.Clear();

            await purchaseOrderRWRepository.UpdateAsync(purchaseOrder, default);

            var dbContext = serviceProvider.GetRequiredService<DbContexts.DbContext>();
            var postSaveDomainEventDispatcher = serviceProvider.GetRequiredService<IPostSaveDomainEventDispatcher>();
            var raisedDomainEventReceiver = serviceProvider.GetRequiredService<IRaisedDomainEventReceiver>();
            var raisedDomainEventCount = await dbContext.Set<RaisedDomainEvent>().CountAsync();

            Assert.AreEqual(1, raisedDomainEventCount);
            Assert.AreEqual(1, preSaveScopedItemContainer.Items.Count);
            Assert.AreEqual(0, postSaveScopedItemContainer.Items.Count);

            var cancellationTokenSource = new CancellationTokenSource();

            await raisedDomainEventReceiver.RunAsync(
                async (raisedDomainEvent, cancellationToken) =>
                {
                    await postSaveDomainEventDispatcher.DispatchAsync(
                        raisedDomainEvent.GetValue(),
                        raisedDomainEvent.GetAmbientContext(),
                        cancellationToken);
                    --raisedDomainEventCount;
                    if (raisedDomainEventCount == 0)
                    {
                        cancellationTokenSource.Cancel();
                    }
                },
                cancellationTokenSource.Token);

            Assert.AreEqual(2, postSaveScopedItemContainer.Items.Count);
        }
    }
}
