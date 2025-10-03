using EventDbLite.Aggregates;

namespace EventDbLite.Abstractions;

public interface IAggregateRepository
{
    T CreateNew<T>(Func<T> constructor) where T : AggregateRoot;

    Task<T?> Get<T>(string id) where T : AggregateRoot, new();
    Task Save<T>(T aggregateRoot) where T : AggregateRoot;
}
