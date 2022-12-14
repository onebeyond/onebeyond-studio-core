using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using EnsureThat;
using FluentValidation;
using MediatR;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Validators;
using OneBeyond.Studio.Domain.SharedKernel.Repositories;

namespace OneBeyond.Studio.Domain.SharedKernel.CommandHandlers;

/// <summary>
/// Handles <see cref="Update{TAggregateRootUpdateDTO, TAggregateRoot, TAggregateRootId}"/> command
/// </summary>
/// <typeparam name="TAggregateRootUpdateDTO"></typeparam>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TAggregateRootId"></typeparam>
public class UpdateHandler<TAggregateRootUpdateDTO, TAggregateRoot, TAggregateRootId>
    : IRequestHandler<Update<TAggregateRootUpdateDTO, TAggregateRoot, TAggregateRootId>, TAggregateRootId>
    where TAggregateRoot : DomainEntity<TAggregateRootId>, IAggregateRoot
    where TAggregateRootId : notnull
{
    /// <summary>
    /// </summary>
    public UpdateHandler(
        IRWRepository<TAggregateRoot, TAggregateRootId> rwRepository,
        IValidator<TAggregateRoot> validator,
        IMapper mapper)
    {
        EnsureArg.IsNotNull(rwRepository, nameof(rwRepository));
        EnsureArg.IsNotNull(validator, nameof(validator));
        EnsureArg.IsNotNull(mapper, nameof(mapper));

        RwRepository = rwRepository;
        Validator = validator;
        Mapper = mapper;
    }

    /// <summary>
    /// </summary>
    protected IRWRepository<TAggregateRoot, TAggregateRootId> RwRepository { get; }

    /// <summary>
    /// </summary>
    protected IValidator<TAggregateRoot> Validator { get; }

    /// <summary>
    /// </summary>
    protected IMapper Mapper { get; }

    /// <summary>
    /// </summary>
    public Task<TAggregateRootId> Handle(
        Update<TAggregateRootUpdateDTO, TAggregateRoot, TAggregateRootId> command,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(command, nameof(command));

        return HandleAsync(command, cancellationToken);
    }

    /// <summary>
    /// </summary>
    protected virtual async Task<TAggregateRootId> HandleAsync(
        Update<TAggregateRootUpdateDTO, TAggregateRoot, TAggregateRootId> command,
        CancellationToken cancellationToken)
    {
        var aggregateRoot = await UpdateAggregateRootFromDTOAsync(
            command.AggregateRootId,
            command.AggregateRootUpdateDTO,
            cancellationToken).ConfigureAwait(false);
        Validator.EnsureIsValid(aggregateRoot);
        await RwRepository.UpdateAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);
        return command.AggregateRootId;
    }

    /// <summary>
    /// </summary>
    protected virtual async Task<TAggregateRoot> UpdateAggregateRootFromDTOAsync(
        TAggregateRootId aggregateRootId,
        TAggregateRootUpdateDTO aggregateRootUpdateDTO,
        CancellationToken cancellationToken)
    {
        var aggregateRoot = await RwRepository.GetByIdAsync(aggregateRootId, cancellationToken).ConfigureAwait(false);
        aggregateRoot = Mapper.Map(aggregateRootUpdateDTO, aggregateRoot);
        return aggregateRoot;
    }
}
