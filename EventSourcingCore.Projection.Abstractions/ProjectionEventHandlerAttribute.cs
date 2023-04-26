using System;
using System.Collections.Generic;
using System.Text;
using Core.Reflection;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Projection.Abstractions
{
    public class ProjectionEventHandlerAttribute : Attribute
    {
        public string Identifier { get; }
        public Type MetadataType { get; }
        public ProjectionEventHandlerAttribute(Type metadataType, string identifier = null)
        {
            if (!metadataType.ImplementsInterface<IEventMetadata>())
            {
                throw new ArgumentException($"{nameof(metadataType)} must implement {nameof(IEventMetadata)}");
            }

            MetadataType = metadataType;
            Identifier = identifier;
        }
    }
}
