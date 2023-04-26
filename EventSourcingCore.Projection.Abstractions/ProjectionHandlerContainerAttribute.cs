using System;
using System.Collections.Generic;
using System.Text;
using Core.Reflection;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Projection.Abstractions
{
    public class ProjectionHandlerContainerAttribute : Attribute
    {
        public Type MetadataType { get; }
        public ProjectionScope Scope { get; }
        public ProjectionHandlerContainerAttribute(Type metadataType, ProjectionScope scope)
        {
            if (!metadataType.ImplementsInterface<IEventMetadata>())
            {
                throw new ArgumentException($"{nameof(metadataType)} must implement {nameof(IEventMetadata)}");
            }

            MetadataType = metadataType;
            Scope = scope;
        }
    }
}
