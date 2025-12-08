namespace OneBeyond.Studio.Core.Mediator.Queries;

/// <summary>
/// Query - for data retrieval only
/// </summary>
/// <typeparam name="TResult">Type of result</typeparam>
public interface IQuery<out TResult> : IRequest
{ } 
