namespace OneBeyond.Studio.Core.Mediator;

/// <summary>
/// Interface to represent a request with a void result
/// </summary>
public interface IRequest : IBaseRequest
{
}

/// <summary>
/// Interface to represent a request with a result
/// </summary>
/// <typeparam name="TResult">Result type</typeparam>
public interface IRequest<out TResult> : IBaseRequest
{
}

public interface IBaseRequest 
{ 
}
