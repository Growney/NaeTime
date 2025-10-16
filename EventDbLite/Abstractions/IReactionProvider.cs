using EventDbLite.Reactions;

namespace EventDbLite.Abstractions;

public interface IReactionProvider : IAsyncDisposable
{
    IDisposable On(Type handlerType, Func<ReactionEvent, Task> handler);
    IAsyncEnumerable<ReactionEvent> StreamSubscription(Type handlerType, CancellationToken cancellationToken);
}
