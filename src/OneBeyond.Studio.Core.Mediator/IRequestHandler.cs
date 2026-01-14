namespace OneBeyond.Studio.Core.Mediator;

public interface IRequestHandler<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    public Task<TResult> Handle(TRequest query, CancellationToken cancellationToken = default);
}

public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    public Task Handle(TRequest command, CancellationToken cancellationToken = default);
}
