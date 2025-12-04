using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Core.Mediator.Commands;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

namespace OneBeyond.Studio.Application.SharedKernel.CommandHandlers;

/// <summary>
/// Handles <see cref="Delete{TAggregateRoot, TAggregateRootId}"/> command
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TAggregateRootId"></typeparam>
public class DeleteHandler<TAggregateRoot, TAggregateRootId>
    : ICommandHandler<Delete<TAggregateRoot, TAggregateRootId>, TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
    where TAggregateRootId : notnull
{
    /// <summary>
    /// </summary>
    public DeleteHandler(IRWRepository<TAggregateRoot, TAggregateRootId> rwRepository)
    {
        EnsureArg.IsNotNull(rwRepository, nameof(rwRepository));

        RwRepository = rwRepository;
    }

    /// <summary>
    /// </summary>
    protected IRWRepository<TAggregateRoot, TAggregateRootId> RwRepository { get; }

    /// <summary>
    /// </summary>
    public Task<TAggregateRootId> HandleAsync(
        Delete<TAggregateRoot, TAggregateRootId> command,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(command, nameof(command));

        return HandleDeleteAsync(command, cancellationToken);
    }

    /// <summary>
    /// </summary>
    protected virtual async Task<TAggregateRootId> HandleDeleteAsync(
        Delete<TAggregateRoot, TAggregateRootId> command,
        CancellationToken cancellationToken)
    {
        var aggregateRoot = await RwRepository.GetByIdAsync(command.AggregateRootId, cancellationToken).ConfigureAwait(false);

        if (aggregateRoot is ISoftDeletable softDeletableAggregateRoot)
        {
            softDeletableAggregateRoot.MarkAsDeleted();
            await RwRepository.UpdateAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await RwRepository.DeleteAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);
        }

        return command.AggregateRootId;
    }
}
