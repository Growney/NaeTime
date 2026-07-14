using NaeTime.Reactions.Abstractions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;

namespace NaeTime.Reactions;

public sealed class EventChannel : IEventChannel
{
    private readonly Channel<EventEnvelope> _channel;

    public EventChannel(Channel<EventEnvelope> channel)
    {
        _channel = channel;
    }

    public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return _channel.Writer.WriteAsync(CreateEnvelope(@event), cancellationToken);
    }

    public bool TryPublish<TEvent>(TEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return _channel.Writer.TryWrite(CreateEnvelope(@event));
    }

    public async IAsyncEnumerable<TEvent> SubscribeAsync<TEvent>([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (EventEnvelope envelope in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            if (envelope.Event is TEvent typedEvent)
            {
                yield return typedEvent;
            }
        }
    }

    private static EventEnvelope CreateEnvelope<TEvent>(TEvent @event)
    {
        return new EventEnvelope(@event!.GetType(), @event, DateTimeOffset.UtcNow);
    }
}
