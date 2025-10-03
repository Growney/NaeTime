using EventDbLite.Streams;

namespace EventDbLite.Abstractions;
public interface IReactionProviderFactory
{
    public IReactionProvider CreateProvider(StreamPosition initialPosition, string? streamName = null);
}
