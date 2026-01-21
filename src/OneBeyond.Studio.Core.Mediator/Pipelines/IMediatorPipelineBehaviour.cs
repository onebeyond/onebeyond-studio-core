namespace OneBeyond.Studio.Core.Mediator.Pipelines;

//TODO RENAME

public interface IMediatorPipelineBehaviour<in TInput, TOutput>
{
    public Task<TOutput> HandleAsync(TInput input, Func<Task<TOutput>> next, CancellationToken cancellationToken = default);    
}

public interface IMediatorPipelineBehaviour<in TInput>
{
    public Task HandleAsync(TInput input, Func<Task> next, CancellationToken cancellationToken = default);
}
