using EventDbLite.Abstractions;
using NaeTime.Collections;
using System.Net.Http.Json;

namespace EventDbLite.Reactions.SignalR.Client;

public class ReactionProvider<TEvent> : IAsyncEnumerable<ReactionEvent<TEvent>>, IEventConsumer
{
    private readonly string? _streamName;
    private readonly StreamPosition _initialPosition;
    private readonly IEventSerializer _eventSerializer;
    private readonly IHttpClientFactory _reactionClientFactory;
    private readonly Action _onCompleted;
    private readonly AwaitableQueue<ReactionEvent<TEvent>> _eventQueue = new(0);

    public ReactionProvider(string? streamName, StreamPosition initialPosition, IEventSerializer eventSerializer, IHttpClientFactory reactionClientFactory, Action onCompleted)
    {
        _streamName = streamName;
        _initialPosition = initialPosition;
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _reactionClientFactory = reactionClientFactory ?? throw new ArgumentNullException(nameof(reactionClientFactory));
        _onCompleted = onCompleted;
    }

    public async IAsyncEnumerator<ReactionEvent<TEvent>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        string identifier = _eventSerializer.GetIdentifier(typeof(TEvent));
        long currentPosition = 0;

        if (_initialPosition != StreamPosition.End)
        {
            using HttpClient reactionClient = _reactionClientFactory.CreateClient();

            string url = string.IsNullOrWhiteSpace(_streamName)
                ? $"/events?position={_initialPosition.Version}"
                : $"/events/{Uri.EscapeDataString(_streamName)}?position={_initialPosition.Version}";

            HttpRequestMessage request = new(HttpMethod.Get, "/events");

            HttpResponseMessage events = await reactionClient.SendAsync(request, cancellationToken);

            events.EnsureSuccessStatusCode();

            IEnumerable<StreamEvent> streamEvents = await events.Content.ReadFromJsonAsync<IEnumerable<StreamEvent>>(
                cancellationToken: cancellationToken) ?? Array.Empty<StreamEvent>();

            foreach (StreamEvent streamEvent in streamEvents)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);

                if (!metadata.Identifier.Equals(identifier))
                {
                    continue;
                }

                object? eventObject = _eventSerializer.DeserializeEvent(streamEvent.Data.Payload, typeof(TEvent));

                if (eventObject is TEvent tEvent)
                {
                    currentPosition = streamEvent.GlobalOrdinal;
                    yield return new ReactionEvent<TEvent>(tEvent, new SubscriptionEvent(false, streamEvent));
                }
            }
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            ReactionEvent<TEvent>? reactionEvent = await _eventQueue.WaitForDequeueAsync(cancellationToken);

            if (reactionEvent == null)
            {
                continue;
            }

            if (reactionEvent.SubscriptionEvent.Event.GlobalOrdinal <= currentPosition)
            {
                continue;
            }

            if (reactionEvent != null)
            {
                yield return reactionEvent;
            }
        }

        _onCompleted();
    }

    public void AddEvent(StreamEvent streamEvent)
    {
        string identifier = _eventSerializer.GetIdentifier(typeof(TEvent));
        EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);
        if (!metadata.Identifier.Equals(identifier))
        {
            return;
        }
        object? eventObject = _eventSerializer.DeserializeEvent(streamEvent.Data.Payload, typeof(TEvent));
        if (eventObject is TEvent tEvent)
        {
            ReactionEvent<TEvent> reactionEvent = new(tEvent, new SubscriptionEvent(true, streamEvent));
            _eventQueue.Enqueue(reactionEvent);
        }
    }
}
