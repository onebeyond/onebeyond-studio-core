namespace OneBeyond.Studio.Core.Mediator.Commands;

/// <summary>
/// Command that returns a result or response.
/// </summary>
/// <typeparam name="TResult">Return type</typeparam>
public interface ICommand<out TResult> : IRequest
{
}


/// <summary>
/// Command that does not return a result or response.
/// </summary>
public interface ICommand : IRequest;
