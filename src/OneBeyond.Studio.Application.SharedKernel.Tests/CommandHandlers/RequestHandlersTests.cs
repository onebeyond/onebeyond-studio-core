using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Application.SharedKernel.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.Tests.CommandHandlers;
using OneBeyond.Studio.Application.SharedKernel.Tests.Infrastructure;
using OneBeyond.Studio.Core.Mediator;
using OneBeyond.Studio.Core.Mediator.DependencyInjection;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;

[TestClass]
public sealed class RequestHandlersTests : TestsBase
{
    [TestMethod]
    public async Task TestCommandWithoutResult()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command = new CommandWithoutResult(); //Just IRequest, no result type

            await mediator.Send(command);

            Assert.HasCount(1, testableContainer);
            Assert.AreEqual(
                typeof(CommandWithoutResultHandler).FullName,
                testableContainer.Dequeue());
        }
    }

    [TestMethod]
    public async Task TestCommandWithResult()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command = new CommandWithResult(); //IRequest with return type: IRequest<string>

            await mediator.Send(command);

            Assert.HasCount(1, testableContainer);
            Assert.AreEqual(
                typeof(CommandWithResultHandler).FullName,
                testableContainer.Dequeue());
        }
    }

    [TestMethod]
    public async Task TestDerivedCommands()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new DerivedCommand1());

            Assert.HasCount(1, testableContainer);
            Assert.AreEqual(
                typeof(DerivedCommand1Handler).FullName,
                testableContainer.Dequeue());

            await mediator.Send(new DerivedCommand2());

            Assert.HasCount(1, testableContainer);
            Assert.AreEqual(
                typeof(DerivedCommand2Handler).FullName,
                testableContainer.Dequeue());

        }
    }

    [TestMethod]
    public async Task TestFactoryCommands()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            //Note, DerivedCommandFactory returns the command as IRequest, not as DerivedCommand1
            await mediator.Send(DerivedCommandFactory.GetCommand(nameof(DerivedCommand1)));

            Assert.HasCount(1, testableContainer);
            Assert.AreEqual(
                typeof(DerivedCommand1Handler).FullName,
                testableContainer.Dequeue());

            //Note, DerivedCommandFactory returns the command as IRequest, not as DerivedCommand2
            await mediator.Send(DerivedCommandFactory.GetCommand(nameof(DerivedCommand2)));

            Assert.HasCount(1, testableContainer);
            Assert.AreEqual(
                typeof(DerivedCommand2Handler).FullName,
                testableContainer.Dequeue());

        }
    }

    protected override void ConfigureTestServices(
        IConfiguration configuration,
        IServiceCollection serviceCollection)
    {
        serviceCollection.AddCoreMediator();
    }

    protected override void ConfigureTestServices(
        IConfiguration configuration,
        ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<Queue<string>>()
            .AsSelf()
            .InstancePerLifetimeScope();

        containerBuilder.AddMediatorRequestHandlers(Assembly.GetExecutingAssembly());
    }
}
