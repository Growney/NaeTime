using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public interface IStreamAggregateRepository : IAggregateRepository
    {
        Task<T> Get<T>(string streamName) where T : IAggregateRoot;
        Task Save(string streamName, IAggregateRoot aggregateRoot);
    }
}
