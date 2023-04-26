using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Projection.Core
{
    public interface IProjectionManagerBuilder
    {
        IServiceProvider Services { get; }
        string Key { get; }

        void Add<EventType, MetadataType>(string identifier, Func<EventType, MetadataType, Task<ProjectionEventResult>> func)
            where EventType : IEvent
            where MetadataType : IEventMetadata;
        void Add<EventType>(string identifier, Func<EventType, Task<ProjectionEventResult>> func) where EventType : IEvent;
        void AddAssemblyProjections(Assembly assembly);
        void AddScoped(Type projectionContainer, Type metadataType);
        void AddScoped<TProjectionContainer, TMetadata>()
            where TMetadata : IEventMetadata;
        void AddSingleton(Type type, Type metadataType);
        void AddSingleton<TProjectionContainer, TMetadata>()
            where TMetadata : IEventMetadata;
        void AddSingleton(Type projectionContainer);
        void AddSingleton<TProjectionContainer>();
        void AddScoped(Type projectionContainer);
        void AddScoped<TProjectionContainer>();
        IProjectionManager Build();
    }
}