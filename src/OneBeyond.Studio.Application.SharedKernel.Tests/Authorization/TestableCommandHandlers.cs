using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using MediatR;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.Authorization;

internal static class TestableCommandHandlers
{
    public sealed class GenericCommandHandler<TCommand>
        : IRequestHandler<TCommand, bool>
        where TCommand : IRequest<bool>
    {
        private readonly Queue<string> _testableContainer;

        public GenericCommandHandler(Queue<string> testableContainer)
        {
            EnsureArg.IsNotNull(testableContainer, nameof(testableContainer));

            _testableContainer = testableContainer;
        }

        public Task<bool> Handle(TCommand request, CancellationToken cancellationToken)
        {
            _testableContainer.Enqueue(GetType().FullName!);
            return Task.FromResult(true);
        }
    }
}
