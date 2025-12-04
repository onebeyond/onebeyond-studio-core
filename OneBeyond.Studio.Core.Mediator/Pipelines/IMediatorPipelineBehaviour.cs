namespace OneBeyond.Studio.Core.Mediator.Pipelines;

public interface IMediatorPipelineBehaviour<in TInput, TOutput>
{
    public Task<TOutput> HandleAsync(TInput input, Func<Task<TOutput>> next, CancellationToken cancellationToken = default);
}
