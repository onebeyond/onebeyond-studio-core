using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;

namespace OneBeyond.Studio.Crosscuts.MessageQueues.InMemory;

internal class InMemoryMessageQueue<TMessage>
   : IMessageQueue<TMessage>
{
    private readonly Queue<TMessage> _queue;

    public InMemoryMessageQueue(Queue<TMessage> queue)
    {
        EnsureArg.IsNotNull(queue, nameof(queue));

        _queue = queue;
    }

    public Task PublishAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        _queue.Enqueue(message);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryMessageQueue<TMessage, TDiscriminator>
    : InMemoryMessageQueue<TMessage>
    , IMessageQueue<TMessage, TDiscriminator>
{
    public InMemoryMessageQueue(Queue<TMessage> queue)
        : base(queue)
    {
    }
}
