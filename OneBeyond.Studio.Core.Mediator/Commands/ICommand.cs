namespace OneBeyond.Studio.Core.Mediator.Commands;

public interface ICommand<out TResult> : ICommand
{
}

public interface ICommand { }
