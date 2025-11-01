using EventDbLite.Streams;

namespace EventDbLite.Abstractions;
public interface IReactionProviderFactory : IDisposable
{
    public IReactionProvider<TEvent> CreateProvider<TEvent>(StreamPosition initialPosition, string? streamName = null);
    public IReactionProvider<TEvent> CreateProvider<TEvent>(StreamPosition initialPosition,IEnumerable<Type> requirements, string? streamName = null);
}
