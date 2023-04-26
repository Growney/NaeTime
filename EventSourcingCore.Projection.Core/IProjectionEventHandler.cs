using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Projection.Core
{
    public interface IProjectionEventHandler
    {
        string Identifier { get; }
        Type EventType { get; }
        Type MetadataType { get; }

        public Task<ProjectionEventResult> Invoke(IServiceProvider serviceScope, IEvent context, IEventMetadata metadata);
    }
}
