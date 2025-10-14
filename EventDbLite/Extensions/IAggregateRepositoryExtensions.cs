using EventDbLite.Abstractions;
using EventDbLite.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDbLite.Abstractions;
public static class IAggregateRepositoryExtensions
{
    public static Task<AggregateType?> Get<AggregateType>(this IAggregateRepository repository) where AggregateType : AggregateRoot, new()
    {
        return repository.Get<AggregateType>(typeof(AggregateType).Name);
    }
    public static Task Save<AggregateType>(this IAggregateRepository repository, AggregateType aggregateRoot) where AggregateType : AggregateRoot
    {
        return repository.Save(aggregateRoot, typeof(AggregateType).Name);
    }
    public static Task<AggregateType?> Get<AggregateType,KeyType>(this IAggregateRepository repository,KeyType key) where AggregateType : AggregateRoot<KeyType>, new()
    {
        return repository.Get<AggregateType>($"{typeof(AggregateType).Name}-{key}");
    }
    public static Task Save<AggregateType,KeyType>(this IAggregateRepository repository, AggregateType aggregateRoot) where AggregateType : AggregateRoot<KeyType>
    {
        return repository.Save(aggregateRoot, $"{typeof(AggregateType).Name}-{aggregateRoot.Id}");
    }
}
