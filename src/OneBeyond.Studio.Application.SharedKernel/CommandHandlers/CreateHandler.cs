using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using EnsureThat;
using FluentValidation;
using MediatR;
using OneBeyond.Studio.Application.SharedKernel.Entities.Validators;
using OneBeyond.Studio.Application.SharedKernel.Repositories;
using OneBeyond.Studio.Domain.SharedKernel.Entities;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Commands;

namespace OneBeyond.Studio.Application.SharedKernel.CommandHandlers;

/// <summary>
/// Handles <see cref="Create{TAggregateRootCreateDTO, TAggregateRoot, TAggregateRootId}"/> command
/// </summary>
/// <typeparam name="TAggregateRootCreateDTO"></typeparam>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TAggregateRootId"></typeparam>
public class CreateHandler<TAggregateRootCreateDTO, TAggregateRoot, TAggregateRootId>
    : IRequestHandler<Create<TAggregateRootCreateDTO, TAggregateRoot, TAggregateRootId>, TAggregateRootId>
    where TAggregateRoot : DomainEntity<TAggregateRootId>, IAggregateRoot
    where TAggregateRootId : notnull
{
    /// <summary>
    /// </summary>
    /// <param name="rwRepository"></param>
    /// <param name="validator"></param>
    /// <param name="mapper"></param>
    public CreateHandler(
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
        Create<TAggregateRootCreateDTO, TAggregateRoot, TAggregateRootId> command,
        CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(command, nameof(command));

        return HandleAsync(command, cancellationToken);
    }

    /// <summary>
    /// </summary>
    protected virtual async Task<TAggregateRootId> HandleAsync(
        Create<TAggregateRootCreateDTO, TAggregateRoot, TAggregateRootId> command,
        CancellationToken cancellationToken)
    {
        var aggregateRoot = await CreateAggregateRootFromDTOAsync(command.AggregateRootCreateDTO, cancellationToken).ConfigureAwait(false);
        Validator.EnsureIsValid(aggregateRoot);
        await RwRepository.CreateAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);
        return aggregateRoot.Id;
    }

    /// <summary>
    /// </summary>
    protected virtual Task<TAggregateRoot> CreateAggregateRootFromDTOAsync(
        TAggregateRootCreateDTO aggregateRootCreateDTO,
        CancellationToken cancellationToken)
    {
        var aggregateRoot = Mapper.Map<TAggregateRootCreateDTO, TAggregateRoot>(aggregateRootCreateDTO);
        return Task.FromResult(aggregateRoot);
    }
}
