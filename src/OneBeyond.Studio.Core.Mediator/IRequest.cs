namespace OneBeyond.Studio.Core.Mediator;

public interface IRequest<out TResult> : IBaseRequest
{
}

public interface IRequest : IBaseRequest;
