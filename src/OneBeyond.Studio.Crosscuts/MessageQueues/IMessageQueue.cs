using System.Threading;
using System.Threading.Tasks;

namespace OneBeyond.Studio.Crosscuts.MessageQueues;

/// <summary>
/// Message queue client.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public interface IMessageQueue<in TMessage>
{
    Task PublishAsync(TMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Message queue client for the cases when there multiple distinguishable
/// queues handling messages of the same type are needed.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
/// <typeparam name="TDiscriminator"></typeparam>
public interface IMessageQueue<in TMessage, TDiscriminator>
    : IMessageQueue<TMessage>
{
}
