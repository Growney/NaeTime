using EventDbLite.Abstractions;
using NaeTime.Collections;
using System.Net.Http.Json;

namespace EventDbLite.Reactions.SignalR.Client;

public class SignalRReactionProvider<TEvent> : IAsyncEnumerable<ReactionEvent<TEvent>>
{
    private readonly string? _streamName;
    private readonly StreamPosition _initialPosition;
    private readonly IEventSerializer _eventSerializer;
    private readonly IHttpClientFactory _reactionClientFactory;
    private readonly IEventClient _eventClient;

    public SignalRReactionProvider(string? streamName, StreamPosition initialPosition, IEventSerializer eventSerializer, IHttpClientFactory reactionClientFactory, IEventClient eventClient)
    {
        _streamName = streamName;
        _initialPosition = initialPosition;
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _reactionClientFactory = reactionClientFactory ?? throw new ArgumentNullException(nameof(reactionClientFactory));
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
    }

    public async IAsyncEnumerator<ReactionEvent<TEvent>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        AwaitableQueue<ReactionEvent<TEvent>> eventQueue = new(0);

        string identifier = _eventSerializer.GetIdentifier(typeof(TEvent));

        Task EventReceived(StreamEvent streamEvent)
        {
            EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);
            if (!metadata.Identifier.Equals(identifier))
            {
                return Task.CompletedTask;
            }
            object? eventObject = _eventSerializer.DeserializeEvent(streamEvent.Data.Payload, typeof(TEvent));
            if (eventObject is TEvent tEvent)
            {
                ReactionEvent<TEvent> reactionEvent = new(tEvent, new SubscriptionEvent(true, streamEvent));
                eventQueue.Enqueue(reactionEvent);
            }

            return Task.CompletedTask;
        }

        _eventClient.OnEventReceived += EventReceived;

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
            ReactionEvent<TEvent>? reactionEvent = await eventQueue.WaitForDequeueAsync(cancellationToken);
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
                Console.WriteLine($"Triggering client event {reactionEvent.SubscriptionEvent.Event.Data.Identifier}");
                yield return reactionEvent;
            }
        }

        _eventClient.OnEventReceived -= EventReceived;
    }
}
