using EventDbLite.Abstractions;
using EventDbLite.Streams;
using KurrentDB.Client;

namespace EventDbLite.KurrentDb;

public class KurrentDbEventStoreLite : IEventStoreLite
{
    private readonly KurrentDBClient _client;

    public KurrentDbEventStoreLite(KurrentDBClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public static StreamState ToKurrentDbState(Abstractions.StreamPosition expectedState)
    {
        if (expectedState == Abstractions.StreamPosition.Any)
        {
            return StreamState.Any;
        }
        if (expectedState == Abstractions.StreamPosition.NoStream)
        {
            return StreamState.NoStream;
        }
        if (expectedState == Abstractions.StreamPosition.StreamExists)
        {
            return StreamState.StreamExists;
        }
        if (expectedState == Abstractions.StreamPosition.Beginning)
        {
            return StreamState.StreamExists;
        }
        if (expectedState == Abstractions.StreamPosition.End)
        {
            return StreamState.Any;
        }
        
        return StreamState.StreamRevision((ulong)expectedState.Version);
    }

    public Task AppendToStreamAsync(string streamName, IEnumerable<Abstractions.EventData> data, Abstractions.StreamPosition expectedState)
    {
        StreamState kurrentDbState = ToKurrentDbState(expectedState);

        IEnumerable<KurrentDB.Client.EventData> kurrentDbEvents = data.Select(e => new KurrentDB.Client.EventData(Uuid.NewUuid(), e.Identifier,e.Payload,e.Metadata));

        return _client.AppendToStreamAsync(streamName, kurrentDbState, kurrentDbEvents);
    }

    public async IAsyncEnumerable<StreamEvent> ReadEvents(StreamDirection direction, Abstractions.StreamPosition fromPosition)
    {
        Direction kurrentDbDirection = direction == StreamDirection.Forward ? Direction.Forwards : Direction.Backwards;

        Position kurrentPosition = new((ulong)fromPosition.Version,(ulong)fromPosition.Version);

        KurrentDBClient.ReadAllStreamResult streamResult = _client.ReadAllAsync(kurrentDbDirection, kurrentPosition);

        await foreach(ResolvedEvent resolvedEvent in streamResult)
        {
            yield return new StreamEvent(resolvedEvent.Event.EventId.ToGuid(),
                resolvedEvent.Event.EventStreamId,
                resolvedEvent.OriginalEvent.EventNumber.ToInt64(),
                (long)resolvedEvent.Event.Position.CommitPosition,
                new Abstractions.EventData(resolvedEvent.Event.Data.ToArray(), resolvedEvent.Event.Metadata.ToArray(), resolvedEvent.Event.EventType)
            );
        }
    }

    public async IAsyncEnumerable<StreamEvent> ReadStreamEvents(string streamName, StreamDirection direction, Abstractions.StreamPosition fromPosition)
    {
        Direction kurrentDbDirection = direction == StreamDirection.Forward ? Direction.Forwards : Direction.Backwards;

        KurrentDB.Client.StreamPosition kurrentPosition = new((ulong)fromPosition.Version);

        KurrentDBClient.ReadStreamResult streamResult = _client.ReadStreamAsync(kurrentDbDirection,streamName, kurrentPosition);

        await foreach (ResolvedEvent resolvedEvent in streamResult)
        {
            yield return new StreamEvent(resolvedEvent.Event.EventId.ToGuid(),
                resolvedEvent.Event.EventStreamId,
                resolvedEvent.OriginalEvent.EventNumber.ToInt64(),
                (long)resolvedEvent.Event.Position.CommitPosition,
                new Abstractions.EventData(resolvedEvent.Event.Data.ToArray(), resolvedEvent.Event.Metadata.ToArray(), resolvedEvent.Event.EventType)
            );
        }
    }

    public IStreamSubscription SubscribeToAllStreams(Abstractions.StreamPosition from)
    {
        FromAll kurrentFrom = FromAll.After(new KurrentDB.Client.Position((ulong)from.Version, (ulong)from.Version));

        KurrentDBClient.StreamSubscriptionResult subscriptionResult = _client.SubscribeToAll(kurrentFrom);
        return new KurrentDbStreamSubscription(subscriptionResult, null);
    }

    public IStreamSubscription SubscribeToStream(string streamName, Abstractions.StreamPosition from)
    {
        FromStream kurrentFrom = FromStream.After((ulong)from.Version);

        KurrentDBClient.StreamSubscriptionResult subscriptionResult = _client.SubscribeToStream(streamName, kurrentFrom);

        return new KurrentDbStreamSubscription(subscriptionResult, streamName);
    }
}
