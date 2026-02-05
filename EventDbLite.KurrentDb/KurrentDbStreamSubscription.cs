using EventDbLite.Abstractions;
using KurrentDB.Client;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EventDbLite.KurrentDb;
public sealed class KurrentDbStreamSubscription : IStreamSubscription
{
    private readonly KurrentDBClient.StreamSubscriptionResult _stream;
    public string? StreamName { get; }

    public KurrentDbStreamSubscription(KurrentDBClient.StreamSubscriptionResult stream, string? streamName)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        StreamName = streamName;
    }

    public async IAsyncEnumerable<SubscriptionEvent> CatchUp(CancellationToken token)
    {
        yield break;
    }

    public async IAsyncEnumerable<SubscriptionEvent> StreamEvents([EnumeratorCancellation]CancellationToken token)
    {
        await foreach (ResolvedEvent resolvedEvent in _stream.WithCancellation(token))
        {
            yield return new SubscriptionEvent(true,
                new(resolvedEvent.Event.EventId.ToGuid(),resolvedEvent.Event.EventStreamId, (long)resolvedEvent.Event.Position.CommitPosition,
                (long)resolvedEvent.Event.Position.CommitPosition,
                new Abstractions.EventData(resolvedEvent.Event.Data.ToArray(), resolvedEvent.Event.Metadata.ToArray(), resolvedEvent.Event.EventType)
                ));
        }
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}
