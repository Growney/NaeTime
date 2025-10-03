using EventDbLite.Streams;

namespace EventDbLite.Abstractions;

public interface IReactionProvider : IAsyncDisposable
{
    IDisposable On(Type handlerType, Func<object, StreamEvent, Task> handler);
    IAsyncEnumerable<object> StreamEvents(Type handlerType, CancellationToken cancellationToken);
}
