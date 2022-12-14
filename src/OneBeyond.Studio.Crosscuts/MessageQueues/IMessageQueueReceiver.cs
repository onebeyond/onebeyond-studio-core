using System;
using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Crosscuts.MessageQueues;

/// <summary>
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public interface IMessageQueueReceiver<out TMessage>
{
    /// <summary>
    /// </summary>
    /// <param name="processAsync"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunAsync(
        Func<TMessage, CancellationToken, Task> processAsync,
        CancellationToken cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="processAsync"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunAsync(
        Func<byte[], CancellationToken, Task> processAsync,
        CancellationToken cancellationToken);
}
