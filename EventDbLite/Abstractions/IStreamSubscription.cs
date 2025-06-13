using EventDbLite.Streams;

namespace EventDbLite.Abstractions;

public interface IStreamSubscription : IDisposable
{
    Task<StreamEvent?> WaitForNextEvent(CancellationToken token);
}
