using EventDbLite.Aggregates;

namespace EventDbLite.Abstractions;

public interface IAggregateRepository
{
    T CreateNew<T>(Func<T>? constructor = null) where T : AggregateRoot, new();

    Task<T?> Get<T>(string streamName) where T : AggregateRoot, new();
    Task Save<T>(T aggregateRoot,string streamName) where T : AggregateRoot;
}
