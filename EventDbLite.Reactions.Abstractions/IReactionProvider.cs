namespace EventDbLite.Reactions.Abstractions;

public interface IReactionProvider<TEvent> : IAsyncEnumerable<ReactionEvent<TEvent>>
{
}
