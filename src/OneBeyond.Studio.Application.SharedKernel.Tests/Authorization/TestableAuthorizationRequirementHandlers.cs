using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.Authorization;
using OneBeyond.Studio.Core.Mediator;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;

internal static class TestableAuthorizationRequirementHandlers
{
    public sealed class Requirement2Handler<TRequest>
        : IAuthorizationRequirementHandler<TestableAuthorizationRequirements.Requirement2, TRequest>
        where TRequest : IBaseRequest
    {
        private readonly Queue<string> _testableContainer;

        public Requirement2Handler(Queue<string> testableContainer)
        {
            EnsureArg.IsNotNull(testableContainer, nameof(testableContainer));

            _testableContainer = testableContainer;
        }

        public Task HandleAsync(
            TestableAuthorizationRequirements.Requirement2 requirement,
            TRequest request,
            CancellationToken cancellationToken)
        {
            _testableContainer.Enqueue(GetType().FullName!);
            return Task.CompletedTask;
        }
    }

    public sealed class Requirement2ViaSomething1Handler<TRequest>
        : IAuthorizationRequirementHandler<TestableAuthorizationRequirements.Requirement2, TRequest>
        where TRequest : TestableCommands.IRequestWithSomething1
    {
        private readonly Queue<string> _testableContainer;

        public Requirement2ViaSomething1Handler(Queue<string> testableContainer)
        {
            EnsureArg.IsNotNull(testableContainer, nameof(testableContainer));

            _testableContainer = testableContainer;
        }

        public Task HandleAsync(
            TestableAuthorizationRequirements.Requirement2 requirement,
            TRequest request,
            CancellationToken cancellationToken)
        {
            _testableContainer.Enqueue(GetType().FullName!);
            return Task.CompletedTask;
        }
    }

    public sealed class Requirement2ViaSomething2Handler<TRequest>
        : IAuthorizationRequirementHandler<TestableAuthorizationRequirements.Requirement2, TRequest>
        where TRequest : TestableCommands.IRequestWithSomething2
    {
        private readonly Queue<string> _testableContainer;

        public Requirement2ViaSomething2Handler(Queue<string> testableContainer)
        {
            EnsureArg.IsNotNull(testableContainer, nameof(testableContainer));

            _testableContainer = testableContainer;
        }

        public Task HandleAsync(
            TestableAuthorizationRequirements.Requirement2 requirement,
            TRequest request,
            CancellationToken cancellationToken)
        {
            _testableContainer.Enqueue(GetType().FullName!);
            return Task.CompletedTask;
        }
    }

    public sealed class Requirement1Handler<TRequest>
        : IAuthorizationRequirementHandler<TestableAuthorizationRequirements.Requirement1, TRequest>
        where TRequest : IBaseRequest
    {
        private readonly Queue<string> _testableContainer;

        public Requirement1Handler(Queue<string> testableContainer)
        {
            EnsureArg.IsNotNull(testableContainer, nameof(testableContainer));

            _testableContainer = testableContainer;
        }

        public Task HandleAsync(
            TestableAuthorizationRequirements.Requirement1 requirement,
            TRequest request,
            CancellationToken cancellationToken)
        {
            if (requirement.IsFailRequested)
            {
                _testableContainer.Enqueue(
                    $"{GetType().FullName}: {requirement} - Failure");
                throw new Exception($"{nameof(TestableAuthorizationRequirements.Requirement1)} fail requested.");
            }
            else
            {
                _testableContainer.Enqueue(
                    $"{GetType().FullName}: {requirement} - Success");
                return Task.CompletedTask;
            }
        }
    }

    public sealed class Requirement3Handler<TRequest>
        : IAuthorizationRequirementHandler<TestableAuthorizationRequirements.Requirement3, TRequest>
        where TRequest : IBaseRequest
    {
        private readonly Queue<string> _testableContainer;

        public Requirement3Handler(Queue<string> testableContainer)
        {
            EnsureArg.IsNotNull(testableContainer, nameof(testableContainer));

            _testableContainer = testableContainer;
        }

        public Task HandleAsync(
            TestableAuthorizationRequirements.Requirement3 requirement,
            TRequest request,
            CancellationToken cancellationToken)
        {
            if (requirement.IsFailRequested)
            {
                _testableContainer.Enqueue(
                    $"{GetType().FullName}: {requirement} - Failure");
                throw new Exception($"{nameof(TestableAuthorizationRequirements.Requirement1)} fail requested.");
            }
            else
            {
                _testableContainer.Enqueue(
                    $"{GetType().FullName}: {requirement} - Success");
                return Task.CompletedTask;
            }
        }
    }

    public sealed class Requirement2ForCommand11Handler
        : IAuthorizationRequirementHandler<TestableAuthorizationRequirements.Requirement2, TestableCommands.Command11>
    {
        private readonly Queue<string> _testableContainer;

        public Requirement2ForCommand11Handler(Queue<string> testableContainer)
        {
            EnsureArg.IsNotNull(testableContainer, nameof(testableContainer));

            _testableContainer = testableContainer;
        }

        public Task HandleAsync(
            TestableAuthorizationRequirements.Requirement2 requirement,
            TestableCommands.Command11 request,
            CancellationToken cancellationToken)
        {
            _testableContainer.Enqueue(GetType().FullName!);
            return Task.CompletedTask;
        }
    }
}
