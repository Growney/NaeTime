using EventDbLite.Streams;

namespace EventDbLite.Abstractions;

public interface IStreamSubscription : IDisposable
{
    IAsyncEnumerable<StreamEvent> StreamEvents(CancellationToken token);
}
