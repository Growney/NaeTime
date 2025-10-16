using EventDbLite.Streams;

namespace EventDbLite.Abstractions;

public interface IStreamSubscription : IDisposable
{
    IAsyncEnumerable<SubscriptionEvent> StreamEvents(CancellationToken token);
}
