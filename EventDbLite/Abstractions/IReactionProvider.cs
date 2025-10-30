using EventDbLite.Reactions;

namespace EventDbLite.Abstractions;

public interface IReactionProvider<TEvent> : IAsyncEnumerable<ReactionEvent<TEvent>>
{
}
