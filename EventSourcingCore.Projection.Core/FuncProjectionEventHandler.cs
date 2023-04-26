using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Projection.Core
{
    public class FuncProjectionEventHandler<TEventType, TMetadataType> : IProjectionEventHandler
        where TMetadataType : IEventMetadata
        where TEventType : IEvent
    {
        public string Identifier { get; }

        public Type EventType => typeof(TEventType);
        public Type MetadataType => typeof(TMetadataType);

        private readonly Func<IServiceProvider, TEventType, TMetadataType, Task<ProjectionEventResult>> _func;

        public FuncProjectionEventHandler(string identifier, Func<IServiceProvider, TEventType, TMetadataType, Task<ProjectionEventResult>> func)
        {
            Identifier = identifier;
            _func = func;
        }

        public Task<ProjectionEventResult> Invoke(IServiceProvider serviceScope, IEvent obj, IEventMetadata metadata)
        {
            return _func(serviceScope, (TEventType)obj, (TMetadataType)metadata);
        }
    }
}
