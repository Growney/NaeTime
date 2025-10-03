using EventDbLite.Abstractions;
using EventDbLite.Streams;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace EventDbLite;

internal class StreamSubscription : IStreamSubscription
{
    private readonly ConcurrentQueue<StreamEvent> _liveQueue = new();
    private readonly IEventStoreLite _eventStore;
    private readonly string? _streamName;
    private readonly StreamPosition _currentPosition;

    private readonly Action<StreamSubscription> _onDispose;
    private readonly SemaphoreSlim _signal = new(0);

    public StreamSubscription(IEventStoreLite eventStore, string? streamName, StreamPosition initialPosition, Action<StreamSubscription> onDispose)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        this._streamName = streamName;
        _currentPosition = initialPosition;
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

    public async IAsyncEnumerable<StreamEvent> StreamEvents([EnumeratorCancellation] CancellationToken token)
    {
        IAsyncEnumerable<StreamEvent> eventStream = _streamName is not null
            ? _eventStore.ReadStreamEvents(_streamName, StreamDirection.Forward, _currentPosition)
            : _eventStore.ReadEvents(StreamDirection.Forward, _currentPosition);

        await foreach (StreamEvent streamEvent in eventStream)
        {
            yield return streamEvent;
        }

        while (!token.IsCancellationRequested)
        {
            if (token.IsCancellationRequested)
            {
                yield break;
            }

            if (_liveQueue.IsEmpty)
            {
                await _signal.WaitAsync(token);
            }

            while (!_liveQueue.IsEmpty)
            {
                if (_liveQueue.TryDequeue(out StreamEvent? streamEvent))
                {
                    yield return streamEvent;
                }
            }
        }
    }
}
