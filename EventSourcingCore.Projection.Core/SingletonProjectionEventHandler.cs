using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Reflection.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Projection.Core
{
    public class SingletonProjectionEventHandler : IProjectionEventHandler
    {
        public string Identifier { get; }

        public Type EventType { get; }
        public Type MetadataType { get; }

        private readonly object _instance;
        private readonly IStandardFuncMethod<IEvent, IEventMetadata, Task<ProjectionEventResult>> _func;

        public SingletonProjectionEventHandler(string identifier, Type metadataType, object instance, IStandardFuncMethod<IEvent, IEventMetadata, Task<ProjectionEventResult>> func)
        {
            Identifier = identifier;
            EventType = func.T1Type;
            MetadataType = func.T2Type ?? metadataType;

            _instance = instance;
            _func = func;

        }

        public Task<ProjectionEventResult> Invoke(IServiceProvider serviceScope, IEvent context, IEventMetadata metadata)
        {
            return _func.Invoke(_instance, context, metadata);
        }
    }
}
