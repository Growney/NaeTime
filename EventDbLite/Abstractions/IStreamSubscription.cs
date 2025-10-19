using EventDbLite.Streams;

namespace EventDbLite.Abstractions;

public interface IStreamSubscription : IDisposable
{
    IAsyncEnumerable<SubscriptionEvent> CatchUp(CancellationToken token);
    IAsyncEnumerable<SubscriptionEvent> StreamEvents(CancellationToken token);
}
