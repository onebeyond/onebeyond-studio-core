namespace OneBeyond.Studio.Core.Mediator.Queries;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles a query. This should be a request for information.
    /// </summary>
    /// <param name="query">Query to be requested</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

