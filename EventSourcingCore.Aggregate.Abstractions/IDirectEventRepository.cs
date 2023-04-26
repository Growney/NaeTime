using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Abstractions
{
    public interface IDirectEventRepository
    {
        Task Save(string stream, IEvent directEvent, IEventMetadata metadata);
        Task Save(string stream, DirectEvent direct);
        Task Save(string stream, IEnumerable<DirectEvent> events);


    }
}
