using EventDbLite.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EventDbLite;

internal class StreamSubscription : IStreamSubscription
{
    private readonly ILogger<StreamSubscription>? _logger;
    private readonly ConcurrentQueue<StreamEvent> _liveQueue = new();
    private readonly IEventStoreLite _eventStore;
    private readonly string? _streamName;
    private StreamPosition _currentPosition;

    private readonly Action<StreamSubscription> _onDispose;
    private readonly SemaphoreSlim _signal = new(0);

    public StreamSubscription(ILogger<StreamSubscription>? logger, IEventStoreLite eventStore, string? streamName, StreamPosition initialPosition, Action<StreamSubscription> onDispose)
    {
        _logger = logger;
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _streamName = streamName;
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

    public async IAsyncEnumerable<SubscriptionEvent> CatchUp([EnumeratorCancellation] CancellationToken token)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        _logger?.LogInformation("Starting catch-up from position {Position} on stream {StreamName}", _currentPosition, _streamName ?? "all streams");
        IAsyncEnumerable<StreamEvent> eventStream = _streamName is not null
            ? _eventStore.ReadStreamEvents(_streamName, StreamDirection.Forward, _currentPosition)
            : _eventStore.ReadEvents(StreamDirection.Forward, _currentPosition);
        await foreach (StreamEvent streamEvent in eventStream.WithCancellation(token))
        {
            yield return new SubscriptionEvent(false, streamEvent);
            _currentPosition = streamEvent.GlobalOrdinal;
        }
        stopwatch.Stop();
        _logger?.LogInformation("Completed catch-up to position {Position} on stream {StreamName} in {ElapsedMilliseconds} ms", _currentPosition, _streamName ?? "all streams", stopwatch.ElapsedMilliseconds);
    }

    public async IAsyncEnumerable<SubscriptionEvent> StreamEvents([EnumeratorCancellation] CancellationToken token)
    {
        IAsyncEnumerable<StreamEvent> eventStream = _streamName is not null
            ? _eventStore.ReadStreamEvents(_streamName, StreamDirection.Forward, _currentPosition)
            : _eventStore.ReadEvents(StreamDirection.Forward, _currentPosition);

        await foreach (StreamEvent streamEvent in eventStream)
        {
            yield return new SubscriptionEvent(false, streamEvent);
            _currentPosition = streamEvent.GlobalOrdinal;
        }

        while (!token.IsCancellationRequested)
        {
            if (token.IsCancellationRequested)
            {
                yield break;
            }

            if (_liveQueue.IsEmpty)
            {
                try
                {
                    await _signal.WaitAsync(token);
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }
            }

            while (!_liveQueue.IsEmpty)
            {
                if (_liveQueue.TryDequeue(out StreamEvent? streamEvent))
                {
                    _logger?.LogTrace("Processing live event {EventId} from stream {StreamName} at position {GlobalOrdinal}", streamEvent.Id, streamEvent.StreamName, streamEvent.GlobalOrdinal);
                    yield return new SubscriptionEvent(true, streamEvent);
                    _currentPosition = streamEvent.GlobalOrdinal;
                }
            }
        }
    }
}
