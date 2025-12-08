namespace OneBeyond.Studio.Core.Mediator.Commands;

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handles a command. This should involve modifying data, or instigating a process.
    /// </summary>
    /// <param name="command">The command to be executed</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Handles a command. This should involve modifying data, or instigating a process.
    /// </summary>
    /// <param name="command">The command to be executed</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
