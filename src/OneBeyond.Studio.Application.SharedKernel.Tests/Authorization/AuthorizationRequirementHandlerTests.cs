using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneBeyond.Studio.Application.SharedKernel.DependencyInjection;
using OneBeyond.Studio.Application.SharedKernel.Exceptions;
using OneBeyond.Studio.Application.SharedKernel.Tests.Infrastructure;
using OneBeyond.Studio.Core.Mediator;
using OneBeyond.Studio.Core.Mediator.DependencyInjection;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;

[TestClass]
public sealed class AuthorizationRequirementHandlerTests : TestsBase
{
    [TestMethod]
    public async Task TestSimpleParameterlessRequirementHandlingSucceeds()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command = new TestableCommands.Command1();

            await mediator.CommandAsync<TestableCommands.Command1, bool>(command);

            Assert.AreEqual(2, testableContainer.Count);
            // Auth handler is executed first
            Assert.AreEqual(
                typeof(TestableAuthorizationRequirementHandlers.Requirement2Handler<TestableCommands.Command1>).FullName,
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.AreEqual(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command1>).FullName,
                testableContainer.Dequeue());
        }
    }

    [TestMethod]
    public async Task TestRequirementHandlingSucceedsWhenHandlerDependsOnCommand()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command2 = new TestableCommands.Command2();

            await mediator.CommandAsync<TestableCommands.Command2, bool>(command2);

            Assert.AreEqual(2, testableContainer.Count);
            // Appropriate (based on the command interface) auth handler is executed first
            Assert.AreEqual(
                typeof(TestableAuthorizationRequirementHandlers.Requirement2ViaSomething1Handler<TestableCommands.Command2>).FullName,
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.AreEqual(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command2>).FullName,
                testableContainer.Dequeue());
        }

        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command3 = new TestableCommands.Command3();

            await mediator.CommandAsync<TestableCommands.Command3, bool>(command3);

            Assert.AreEqual(2, testableContainer.Count);
            // Appropriate (based on the command interface) auth handler is executed first
            Assert.AreEqual(
                typeof(TestableAuthorizationRequirementHandlers.Requirement2ViaSomething2Handler<TestableCommands.Command3>).FullName,
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.AreEqual(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command3>).FullName,
                testableContainer.Dequeue());
        }
    }

    [TestMethod]
    public async Task TestPolicyRequirementsAreHandledByOrLogicAndPolicySucceedsEvenFirstRequirementFails()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command4 = new TestableCommands.Command4();

            await mediator.CommandAsync<TestableCommands.Command4, bool>(command4);

            Assert.AreEqual(3, testableContainer.Count);
            // First requirement handler is executed and fails
            Assert.AreEqual(
                $"{typeof(TestableAuthorizationRequirementHandlers.Requirement1Handler<TestableCommands.Command4>).FullName}: {new TestableAuthorizationRequirements.Requirement1(true, 42, "Forty two")} - Failure",
                testableContainer.Dequeue());
            // Second requirement handler is executed and succeeds
            Assert.AreEqual(
                $"{typeof(TestableAuthorizationRequirementHandlers.Requirement3Handler<TestableCommands.Command4>).FullName}: {new TestableAuthorizationRequirements.Requirement3(false)} - Success",
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.AreEqual(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command4>).FullName,
                testableContainer.Dequeue());
        }
    }

    [TestMethod]
    public async Task TestPolicyRequirementsAreHandlerByOrLogicAndSecondRequirementNotCheckedWhenFirstOneSucceeds()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command5 = new TestableCommands.Command5();

            await mediator.CommandAsync<TestableCommands.Command5, bool>(command5);

            Assert.AreEqual(2, testableContainer.Count);
            // First requirement handler is executed and succeeds
            Assert.AreEqual(
                $"{typeof(TestableAuthorizationRequirementHandlers.Requirement2Handler<TestableCommands.Command5>).FullName}",
                testableContainer.Dequeue());
            // Second requirement handler is not executed
            // Command handler is executed last
            Assert.AreEqual(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command5>).FullName,
                testableContainer.Dequeue());
        }
    }

    [TestMethod]
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
                await mediator.CommandAsync<TestableCommands.Command9, bool>(command9);
                Assert.Fail();
            }
            catch (AuthorizationPolicyFailedException)
            {
                Assert.AreEqual(2, testableContainer.Count);
                // First requirement handler is executed and fails
                Assert.AreEqual(
                    $"{typeof(TestableAuthorizationRequirementHandlers.Requirement1Handler<TestableCommands.Command9>).FullName}: {new TestableAuthorizationRequirements.Requirement1(true, 41, "Forty one")} - Failure",
                    testableContainer.Dequeue());
                // Second requirement handler is executed and succeeds
                Assert.AreEqual(
                    $"{typeof(TestableAuthorizationRequirementHandlers.Requirement3Handler<TestableCommands.Command9>).FullName}: {new TestableAuthorizationRequirements.Requirement3(true)} - Failure",
                    testableContainer.Dequeue());
                // Command handler is not executed
            }
        }
    }

    [TestMethod]
    public async Task TestPoliciesAreHandledByAndLogicAndEntireCheckSucceedsWhenBothPoliciesSucceed()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var testableContainer = serviceProvider.GetRequiredService<Queue<string>>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command6 = new TestableCommands.Command6();

            await mediator.CommandAsync<TestableCommands.Command6, bool>(command6);

            Assert.AreEqual(3, testableContainer.Count);
            // First requirement handler is executed and succeeds
            Assert.AreEqual(
                $"{typeof(TestableAuthorizationRequirementHandlers.Requirement2Handler<TestableCommands.Command6>).FullName}",
                testableContainer.Dequeue());
            // Second requirement handler is executed and succeeds
            Assert.AreEqual(
                $"{typeof(TestableAuthorizationRequirementHandlers.Requirement3Handler<TestableCommands.Command6>).FullName}: {new TestableAuthorizationRequirements.Requirement3(false)} - Success",
                testableContainer.Dequeue());
            // Command handler is executed last
            Assert.AreEqual(
                typeof(TestableCommandHandlers.GenericCommandHandler<TestableCommands.Command6>).FullName,
                testableContainer.Dequeue());
        }
    }

    [TestMethod]
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
                await mediator.CommandAsync<TestableCommands.Command7, bool>(command7);
                Assert.Fail();
            }
            catch (AuthorizationPolicyFailedException)
            {
                Assert.AreEqual(1, testableContainer.Count);
                // First requirement handler is executed and fails
                Assert.AreEqual(
                    $"{typeof(TestableAuthorizationRequirementHandlers.Requirement3Handler<TestableCommands.Command7>).FullName}: {new TestableAuthorizationRequirements.Requirement3(true)} - Failure",
                    testableContainer.Dequeue());
                // Second requirement handler is not executed
                // Command handler is not executed
            }
        }
    }

    [TestMethod]
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
                await mediator.CommandAsync<TestableCommands.Command8, bool>(command8);
                Assert.Fail();
            }
            catch (AuthorizationPolicyFailedException)
            {
                Assert.AreEqual(2, testableContainer.Count);
                // First requirement handler is executed and succeeds
                Assert.AreEqual(
                    $"{typeof(TestableAuthorizationRequirementHandlers.Requirement1Handler<TestableCommands.Command8>).FullName}: {new TestableAuthorizationRequirements.Requirement1(false, 45, "Forty five")} - Success",
                    testableContainer.Dequeue());
                // Second requirement handler is executed and fails
                Assert.AreEqual(
                    $"{typeof(TestableAuthorizationRequirementHandlers.Requirement3Handler<TestableCommands.Command8>).FullName}: {new TestableAuthorizationRequirements.Requirement3(true)} - Failure",
                    testableContainer.Dequeue());
                // Command handler is not executed
            }
        }
    }

    [TestMethod]
    public async Task TestRequestsWithoutPolicyAssignedToThemFail()
    {
        using (var serviceScope = ServiceProvider.CreateScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command10 = new TestableCommands.Command10();

            try
            {
                await mediator.CommandAsync<TestableCommands.Command10, bool>(command10);
            }
            catch (AuthorizationPolicyMissingException exception)
            {
                Assert.AreEqual(command10.GetType(), exception.RequestType);
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
