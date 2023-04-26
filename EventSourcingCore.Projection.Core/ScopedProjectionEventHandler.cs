using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Core.Reflection.Abstractions;
using Core.Security.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Projection.Core
{
    public class ScopedProjectionEventHandler : IProjectionEventHandler
    {
        public string Identifier { get; }

        public Type EventType { get; }
        public Type MetadataType { get; }

        private readonly IStandardFuncMethod<IEvent, IEventMetadata, Task<ProjectionEventResult>> _method;

        public ScopedProjectionEventHandler(string identifier, Type metadataType, IStandardFuncMethod<IEvent, IEventMetadata, Task<ProjectionEventResult>> standardMethod)
        {
            Identifier = identifier;
            EventType = standardMethod.T1Type;
            MetadataType = standardMethod.T2Type ?? metadataType;
            _method = standardMethod;
        }

        public Task<ProjectionEventResult> Invoke(IServiceProvider serviceProvider, IEvent eventObj, IEventMetadata metadata)
        {
            return _method.Invoke(serviceProvider, eventObj, metadata);
        }
    }
}
