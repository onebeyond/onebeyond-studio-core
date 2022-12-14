using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

namespace OneBeyond.Studio.Application.SharedKernel.Tests.CommandHandlers;

internal sealed class ClosedCreateHandler : IRequestHandler<Create<SomeDto, SomeAggregateRoot, int>, int>
{
    public Task<int> Handle(Create<SomeDto, SomeAggregateRoot, int> request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
