namespace OneBeyond.Studio.Core.Mediator;

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    public Task<TResult> HandleAsync(TCommand query, CancellationToken cancellationToken = default);
}
