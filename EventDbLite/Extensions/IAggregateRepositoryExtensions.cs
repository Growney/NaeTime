using EventDbLite.Aggregates;
using EventDbLite.Streams;

namespace EventDbLite.Abstractions;
public static class IAggregateRepositoryExtensions
{
    public static Task<AggregateType?> Get<AggregateType, KeyType>(this IAggregateRepository repository, KeyType key) where AggregateType : AggregateRoot<KeyType>, new()
    {
        return repository.Get<AggregateType>($"{typeof(AggregateType).Name}-{key}");
    }
    public static Task Save<AggregateType, KeyType>(this IAggregateRepository repository, AggregateType aggregateRoot) where AggregateType : AggregateRoot<KeyType>
    {
        return repository.Save(aggregateRoot, $"{typeof(AggregateType).Name}-{aggregateRoot.Id}", StreamPosition.WithVersion(aggregateRoot.Version));
    }
    public static Task Save<AggregateType, KeyType>(this IAggregateRepository repository, AggregateType aggregateRoot, StreamPosition expectedPosition) where AggregateType : AggregateRoot<KeyType>
    {
        return repository.Save(aggregateRoot, $"{typeof(AggregateType).Name}-{aggregateRoot.Id}", expectedPosition);
    }
    public static Task Save<AggregateType>(this IAggregateRepository repository, AggregateType aggregateRoot, StreamPosition expectedPosition) where AggregateType : AggregateRoot<Guid>
    {
        return repository.Save<AggregateType, Guid>(aggregateRoot, expectedPosition);
    }
    public static Task Save<AggregateType>(this IAggregateRepository repository, AggregateType aggregateRoot) where AggregateType : AggregateRoot<Guid>
    {
        return repository.Save<AggregateType, Guid>(aggregateRoot, StreamPosition.WithVersion(aggregateRoot.Version));
    }
    public static Task Save<AggregateType>(this IAggregateRepository repository, AggregateType aggregateRoot, string streamName) where AggregateType : AggregateRoot
    {
        return repository.Save<AggregateType>(aggregateRoot, streamName, StreamPosition.WithVersion(aggregateRoot.Version));
    }
}
