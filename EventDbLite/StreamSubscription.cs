using EventDbLite.Abstractions;
using EventDbLite.Streams;
using System.Collections.Concurrent;

namespace EventDbLite;

internal class StreamSubscription : IStreamSubscription
{
    private readonly ConcurrentQueue<StreamEvent> _liveQueue = new();
    private readonly IAsyncEnumerable<StreamEvent> _catchUpQueue;
    private readonly Action<StreamSubscription> _onDispose;
    private readonly SemaphoreSlim _signal = new(0);

    public StreamSubscription(IAsyncEnumerable<StreamEvent> initialEvents, Action<StreamSubscription> onDispose)
    {
        _catchUpQueue = initialEvents ?? throw new ArgumentNullException(nameof(initialEvents));
        _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
    }
    public void AddLiveEvent(StreamEvent streamEvent)
    {
        _liveQueue.Enqueue(streamEvent);
        _signal.Release();
    }

    public void Dispose()
    {
        _onDispose(this);
        _signal.Dispose();
    }

    public async Task<StreamEvent?> WaitForNextEvent(CancellationToken token)
    {
        await foreach (StreamEvent catchUpEvent in _catchUpQueue.WithCancellation(token))
        {
            return catchUpEvent;
        }

        if (_liveQueue.IsEmpty)
        {
            await _signal.WaitAsync(token);
        }

        return _liveQueue.TryDequeue(out StreamEvent? streamEvent)
            ? streamEvent
            : default;
    }
}
