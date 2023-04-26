using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Core.Factories
{
    public interface IEventMetadataFactory<MetadataType> where MetadataType : IEventMetadata
    {
        MetadataType CreateMetadata(IEvent eventObj);
    }
}
