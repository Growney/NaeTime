using EventDbLite.Aggregates;
using EventDbLite.Streams;

namespace EventDbLite.Abstractions;

public interface IAggregateRepository
{
    T CreateNew<T>(Func<T>? constructor = null) where T : AggregateRoot, new();

    Task<T?> Get<T>(string streamName) where T : AggregateRoot, new();
    Task Save<T>(T aggregateRoot, string streamName, StreamPosition expectedPosition) where T : AggregateRoot;
}
