using EventDbLite.Streams;

namespace EventDbLite.Abstractions;
public interface IReactionProviderFactory
{
    public IReactionProvider CreateProvider(StreamPosition initialPosition, string? streamName = null);
    public Task<IReactionProvider> CreateProviderAsync(StreamPosition initialPosition, Func<Task>? onStart = null, string? streamName = null);
}
