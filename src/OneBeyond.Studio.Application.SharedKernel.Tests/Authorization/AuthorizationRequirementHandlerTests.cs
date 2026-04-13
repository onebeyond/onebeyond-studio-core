using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;
using OneBeyond.Studio.Application.SharedKernel.Tests.Infrastructure;
using OneBeyond.Studio.Core.Mediator;
using OneBeyond.Studio.Core.Mediator.DependencyInjection;
using Xunit;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;


public sealed class AuthorizationRequirementHandlerTests : TestsBase
{
    [Fact]
    public async Task TestSimpleParameterlessRequirementHandlingSucceeds()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command = new TestableCommands.Command1();

            await mediator.Send(command, TestContext.Current.CancellationToken);

            Assert.Equal(2, testableContainer.Count());
            // Auth handler is executed first
            Assert.Equal(
                typeof(TestableAuthorizationRequirementHandlers.Requirement2Handler<TestableCommands.Command1>).FullName,
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.Equal(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command1>).FullName,
                testableContainer.Dequeue());
        }
    }

    [Fact]
    public async Task TestSimpleParameterlessRequirementForConcreteCommandHandlingSucceeds()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command = new TestableCommands.Command11();

            //Note! Mediator has separate methods for 
            //public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            //and
            //public Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
            //we need to test authorization requirements for both

            await mediator.Send(command, TestContext.Current.CancellationToken); //Note, here we pass command as TestableCommands.Command11, not as IRequest

            Assert.Equal(2, testableContainer.Count());
            // Auth handler is executed first
            Assert.Equal(
                typeof(TestableAuthorizationRequirementHandlers.Requirement2ForCommand11Handler).FullName,
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.Equal(
                typeof(TestableCommandHandlers.Command11Handler).FullName,
                testableContainer.Dequeue());
        }
    }

    [Fact]
    public async Task TestSimpleParameterlessRequirementForConcreteCommandAsIRequestHandlingSucceeds()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            IRequest command = new TestableCommands.Command11();

            //Note! Mediator has separate methods for 
            //public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            //and
            //public Task Send(IRequest request, CancellationToken cancellationToken = default)
            //we need to test authorization requirements for both

            await mediator.Send(command, TestContext.Current.CancellationToken); //Note, here we pass command as IRequest, not as TestableCommands.Command11

            Assert.Equal(2, testableContainer.Count());
            // Auth handler is executed first
            Assert.Equal(
                typeof(TestableAuthorizationRequirementHandlers.Requirement2ForCommand11Handler).FullName,
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.Equal(
                typeof(TestableCommandHandlers.Command11Handler).FullName,
                testableContainer.Dequeue());
        }
    }

    [Fact]
    public async Task TestRequirementHandlingSucceedsWhenHandlerDependsOnCommand()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command2 = new TestableCommands.Command2();

            await mediator.Send(command2, TestContext.Current.CancellationToken);

            Assert.Equal(2, testableContainer.Count());
            // Appropriate (based on the command interface) auth handler is executed first
            Assert.Equal(
                typeof(TestableAuthorizationRequirementHandlers.Requirement2ViaSomething1Handler<TestableCommands.Command2>).FullName,
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.Equal(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command2>).FullName,
                testableContainer.Dequeue());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command3 = new TestableCommands.Command3();

            await mediator.Send(command3, TestContext.Current.CancellationToken);

            Assert.Equal(2, testableContainer.Count());
            // Appropriate (based on the command interface) auth handler is executed first
            Assert.Equal(
                typeof(TestableAuthorizationRequirementHandlers.Requirement2ViaSomething2Handler<TestableCommands.Command3>).FullName,
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.Equal(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command3>).FullName,
                testableContainer.Dequeue());
        }
    }

    [Fact]
    public async Task TestPolicyRequirementsAreHandledByOrLogicAndPolicySucceedsEvenFirstRequirementFails()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command4 = new TestableCommands.Command4();

            await mediator.Send(command4, TestContext.Current.CancellationToken);

            Assert.Equal(3, testableContainer.Count());
            // First requirement handler is executed and fails
            Assert.Equal(
                $"{typeof(TestableAuthorizationRequirementHandlers.Requirement1Handler<TestableCommands.Command4>).FullName}: {new TestableAuthorizationRequirements.Requirement1(true, 42, "Forty two")} - Failure",
                testableContainer.Dequeue());
            // Second requirement handler is executed and succeeds
            Assert.Equal(
                $"{typeof(TestableAuthorizationRequirementHandlers.Requirement3Handler<TestableCommands.Command4>).FullName}: {new TestableAuthorizationRequirements.Requirement3(false)} - Success",
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.Equal(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command4>).FullName,
                testableContainer.Dequeue());
        }
    }

    [Fact]
    public async Task TestPolicyRequirementsAreHandlerByOrLogicAndSecondRequirementNotCheckedWhenFirstOneSucceeds()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command5 = new TestableCommands.Command5();

            await mediator.Send(command5, TestContext.Current.CancellationToken);

            Assert.Equal(2, testableContainer.Count());
            // First requirement handler is executed and succeeds
            Assert.Equal(
                $"{typeof(TestableAuthorizationRequirementHandlers.Requirement2Handler<TestableCommands.Command5>).FullName}",
                testableContainer.Dequeue());
            // Second requirement handler is not executed
            // Command handler is executed last
            Assert.Equal(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command5>).FullName,
                testableContainer.Dequeue());
        }
    }

    [Fact]
    public async Task TestPolicyRequirementsAreHandledByOrLogicAndPolicyFailsWhenBothRequirementsFail()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command9 = new TestableCommands.Command9();

            try
            {
                await mediator.Send(command9, TestContext.Current.CancellationToken);
                Assert.Fail();
            }
            catch (AuthorizationPolicyFailedException)
            {
                Assert.Equal(2, testableContainer.Count());
                // First requirement handler is executed and fails
                Assert.Equal(
                    $"{typeof(TestableAuthorizationRequirementHandlers.Requirement1Handler<TestableCommands.Command9>).FullName}: {new TestableAuthorizationRequirements.Requirement1(true, 41, "Forty one")} - Failure",
                    testableContainer.Dequeue());
                // Second requirement handler is executed and succeeds
                Assert.Equal(
                    $"{typeof(TestableAuthorizationRequirementHandlers.Requirement3Handler<TestableCommands.Command9>).FullName}: {new TestableAuthorizationRequirements.Requirement3(true)} - Failure",
                    testableContainer.Dequeue());
                // Command handler is not executed
            }
        }
    }

    [Fact]
    public async Task TestPoliciesAreHandledByAndLogicAndEntireCheckSucceedsWhenBothPoliciesSucceed()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command6 = new TestableCommands.Command6();

            await mediator.Send(command6, TestContext.Current.CancellationToken);

            Assert.Equal(3, testableContainer.Count());
            // First requirement handler is executed and succeeds
            Assert.Equal(
                $"{typeof(TestableAuthorizationRequirementHandlers.Requirement2Handler<TestableCommands.Command6>).FullName}",
                testableContainer.Dequeue());
            // Second requirement handler is executed and succeeds
            Assert.Equal(
                $"{typeof(TestableAuthorizationRequirementHandlers.Requirement3Handler<TestableCommands.Command6>).FullName}: {new TestableAuthorizationRequirements.Requirement3(false)} - Success",
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.Equal(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command6>).FullName,
                testableContainer.Dequeue());
        }
    }

    [Fact]
    public async Task TestPoliciesAreHandledByAndLogicAndEntireCheckFailsWhenFirstPolicyFailWhileSecondOneNotExecutedAtAll()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command7 = new TestableCommands.Command7();

            try
            {
                await mediator.Send(command7, TestContext.Current.CancellationToken);
                Assert.Fail();
            }
            catch (AuthorizationPolicyFailedException)
            {
                Assert.Single(testableContainer);
                // First requirement handler is executed and fails
                Assert.Equal(
                    $"{typeof(TestableAuthorizationRequirementHandlers.Requirement3Handler<TestableCommands.Command7>).FullName}: {new TestableAuthorizationRequirements.Requirement3(true)} - Failure",
                    testableContainer.Dequeue());
                // Second requirement handler is not executed
                // Command handler is not executed
            }
        }
    }

    [Fact]
    public async Task TestPoliciesAreHandledByAndLogicAndEntireCheckFailsWhenFirstPolicySucceedsWhileSecondOneFails()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command8 = new TestableCommands.Command8();

            try
            {
                await mediator.Send(command8, TestContext.Current.CancellationToken);
                Assert.Fail();
            }
            catch (AuthorizationPolicyFailedException)
            {
                Assert.Equal(2, testableContainer.Count());
                // First requirement handler is executed and succeeds
                Assert.Equal(
                    $"{typeof(TestableAuthorizationRequirementHandlers.Requirement1Handler<TestableCommands.Command8>).FullName}: {new TestableAuthorizationRequirements.Requirement1(false, 45, "Forty five")} - Success",
                    testableContainer.Dequeue());
                // Second requirement handler is executed and fails
                Assert.Equal(
                    $"{typeof(TestableAuthorizationRequirementHandlers.Requirement3Handler<TestableCommands.Command8>).FullName}: {new TestableAuthorizationRequirements.Requirement3(true)} - Failure",
                    testableContainer.Dequeue());
                // Command handler is not executed
            }
        }
    }

    [Fact]
    public async Task TestRequestsWithoutPolicyAssignedToThemFail()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command10 = new TestableCommands.Command10();

            try
            {
                await mediator.Send(command10, TestContext.Current.CancellationToken);
            }
            catch (AuthorizationPolicyMissingException exception)
            {
                Assert.Equal(command10.GetType(), exception.RequestType);
            }
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

        containerBuilder.RegisterGeneric(typeof(TestableCommandHandlers.GenericCommandHandler<>))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        containerBuilder.AddAuthorizationRequirementHandlers(
            new SharedKernel.Authorization.AuthorizationOptions
            {
                AllowUnattributedRequests = false
            },
            Assembly.GetExecutingAssembly());
    }
}


