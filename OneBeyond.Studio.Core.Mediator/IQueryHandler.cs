namespace OneBeyond.Studio.Core.Mediator;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

