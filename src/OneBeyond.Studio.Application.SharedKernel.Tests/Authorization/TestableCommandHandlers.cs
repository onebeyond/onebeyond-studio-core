using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Core.Mediator.Commands;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;

internal static class TestableCommandHandlers
{
    public sealed class GenericCommandHandler<TCommand>
        : ICommandHandler<TCommand, bool>
        where TCommand : ICommand<bool>
    {
        private readonly Queue<string> _testableContainer;

        public GenericCommandHandler(Queue<string> testableContainer)
        {
            EnsureArg.IsNotNull(testableContainer, nameof(testableContainer));

            _testableContainer = testableContainer;
        }

        public Task<bool> HandleAsync(TCommand request, CancellationToken cancellationToken)
        {
            _testableContainer.Enqueue(GetType().FullName!);
            return Task.FromResult(true);
        }
    }
}
