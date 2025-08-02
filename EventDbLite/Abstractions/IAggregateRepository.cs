using EventDbLite.Aggregates;

namespace EventDbLite.Abstractions;

public interface IAggregateRepository
{
    Task<T?> Get<T>(Guid id) where T : AggregateRoot, new();
    Task Save<T>(T aggregateRoot) where T : AggregateRoot;
}
