using EventDbLite.Aggregates;

namespace EventDbLite.Abstractions;

public interface IAggregateRepository
{
    T CreateNew<T,K>(Func<T>? constructor = null) where T : AggregateRoot<K>, new();

    Task<T?> Get<T,K>(K? id = default) where T : AggregateRoot<K>, new();
    Task Save<T,K>(T aggregateRoot) where T : AggregateRoot<K>;
}
