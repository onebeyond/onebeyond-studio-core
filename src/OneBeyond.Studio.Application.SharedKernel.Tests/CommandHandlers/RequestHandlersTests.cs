using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.Tests.CommandHandlers;
using OneBeyond.Studio.Application.SharedKernel.Tests.Infrastructure;
using OneBeyond.Studio.Core.Mediator;
using OneBeyond.Studio.Core.Mediator.DependencyInjection;
using Xunit;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;


public sealed class RequestHandlersTests : TestsBase
{
    [Fact]
    public async Task TestCommandWithoutResult()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command = new CommandWithoutResult(); //Just IRequest, no result type

            await mediator.Send(command, TestContext.Current.CancellationToken);

            Assert.Single(testableContainer);
            Assert.Equal(
                typeof(CommandWithoutResultHandler).FullName,
                testableContainer.Dequeue());
        }
    }

    [Fact]
    public async Task TestCommandWithResult()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command = new CommandWithResult(); //IRequest with return type: IRequest<string>

            await mediator.Send(command, TestContext.Current.CancellationToken);

            Assert.Single(testableContainer);
            Assert.Equal(
                typeof(CommandWithResultHandler).FullName,
                testableContainer.Dequeue());
        }
    }

    [Fact]
    public async Task TestDerivedCommands()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new DerivedCommand1(), TestContext.Current.CancellationToken);

            Assert.Single(testableContainer);
            Assert.Equal(
                typeof(DerivedCommand1Handler).FullName,
                testableContainer.Dequeue());

            await mediator.Send(new DerivedCommand2(), TestContext.Current.CancellationToken);

            Assert.Single(testableContainer);
            Assert.Equal(
                typeof(DerivedCommand2Handler).FullName,
                testableContainer.Dequeue());

        }
    }

    [Fact]
    public async Task TestFactoryCommands()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            //Note, DerivedCommandFactory returns the command as IRequest, not as DerivedCommand1
            await mediator.Send(DerivedCommandFactory.GetCommand(nameof(DerivedCommand1)), TestContext.Current.CancellationToken);

            Assert.Single(testableContainer);
            Assert.Equal(
                typeof(DerivedCommand1Handler).FullName,
                testableContainer.Dequeue());

            //Note, DerivedCommandFactory returns the command as IRequest, not as DerivedCommand2
            await mediator.Send(DerivedCommandFactory.GetCommand(nameof(DerivedCommand2)), TestContext.Current.CancellationToken);

            Assert.Single(testableContainer);
            Assert.Equal(
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


