using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Event.Core;

namespace EventSourcingCore.Projection.Core
{
    public interface IProjectionEventHandlerRepository<MetadataType> where MetadataType : IEventMetadata
    {
        IEnumerable<IProjectionEventHandler> GetHandlers(string identifier);
        void RegisterProjection(Type aggregateType);
    }
}
