using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Core.Reflection;
using Core.Reflection.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Projection.Abstractions;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Projection.Core
{
    public class ProjectionManagerBuilder : IProjectionManagerBuilder
    {
        private readonly MethodStandard _standard = new MethodStandard(
            name: "When",
            overrideAttributes: new Type[] { typeof(ProjectionEventHandlerAttribute) });


        private readonly ILogger<ProjectionManagerBuilder> _logger;

        private readonly List<Action<IProjectionManager>> _registrationMethods = new List<Action<IProjectionManager>>();
        public string Key { get; }
        public string StreamName { get; }
        public IServiceProvider Services { get; }
        public Func<IServiceProvider, IProjectionPositionRepository> PositionRepository { get; set; }


        public ProjectionManagerBuilder(string key, string streamName, IServiceProvider provider)
        {
            Key = key;
            Services = provider;
            StreamName = streamName;

            _logger = provider.GetService<ILogger<ProjectionManagerBuilder>>();
        }

        public IProjectionManager Build()
        {

            var connection = Services.GetService<IEventStoreSubscriptionConnection>();
            var factory = Services.GetService<IEventFactory>();
            var logger = Services.GetService<ILogger<ProjectionManager>>();

            var positionRepo = PositionRepository?.Invoke(Services) ?? Services.GetService<IProjectionPositionRepository>();
            _logger.LogInformation("Building projection manager {managerKey} with {handlerCount} handlers", Key, _registrationMethods.Count);
            var manager = new ProjectionManager(Key, StreamName, connection, factory, Services, positionRepo, logger);

            foreach (var method in _registrationMethods)
            {
                method.Invoke(manager);
            }

            return manager;
        }

        public void Add<EventType>(string identifier, Func<EventType, Task<ProjectionEventResult>> func)
            where EventType : IEvent
        {
            _logger.LogInformation("Adding function handler for {identifier} to projection manager builder for {managerKey}", identifier, Key);
            var handler = new FuncProjectionEventHandler<EventType, EventMetadata>(identifier,
                (provider, eventObj, metadata) => func(eventObj));

            _registrationMethods.Add(x => x.Register(handler));
        }

        public void Add<EventType, MetadataType>(string identifier, Func<EventType, MetadataType, Task<ProjectionEventResult>> func)
            where EventType : IEvent
            where MetadataType : IEventMetadata
        {
            _logger.LogInformation("Adding function handler for {identifier} to projection manager builder for {managerKey} ", identifier, Key);
            var handler = new FuncProjectionEventHandler<EventType, MetadataType>(identifier,
                (provider, eventObj, metadata) => func(eventObj, metadata));

            _registrationMethods.Add(x => x.Register(handler));
        }
        public void Add<ProjectionHandlerContainer>()
        {
            Add(typeof(ProjectionHandlerContainer), true);
        }
        public void Add(Type type)
        {
            Add(type, true);
        }
        private void Add(Type type, bool throwOnNoAttribute)
        {
            var attribute = type.GetCustomAttribute<ProjectionHandlerContainerAttribute>();
            if (attribute != null)
            {
                switch (attribute.Scope)
                {
                    case ProjectionScope.Singleton:
                        AddSingleton(type, attribute.MetadataType);
                        break;
                    case ProjectionScope.Scoped:
                        AddScoped(type, attribute.MetadataType);
                        break;
                    default:
                        break;
                }
            }
            else if (throwOnNoAttribute)
            {
                throw new ArgumentException($"{nameof(type)} mus have the {nameof(ProjectionHandlerContainerAttribute)} attribute assigned");
            }
        }
        private string GetIdentifier(IStandardFuncMethod<IEvent, IEventMetadata, Task<ProjectionEventResult>> method)
        {
            foreach (var attribute in method.Attributes)
            {
                if (attribute is ProjectionEventHandlerAttribute handlerAttribute)
                {
                    if (!string.IsNullOrWhiteSpace(handlerAttribute.Identifier))
                    {
                        return handlerAttribute.Identifier ?? method.T1Type.GetEventIdentifier();
                    }
                }
            }

            return method.T1Type.GetEventIdentifier();
        }
        private void CheckAndThrowMetadataType(Type metadataType)
        {
            if (!metadataType.ImplementsInterface<IEventMetadata>())
            {
                throw new ArgumentException($"{nameof(metadataType)} must implement {nameof(IEventMetadata)}");
            }
        }
        public void AddSingleton(Type classType, Type metadataType)
        {
            CheckAndThrowMetadataType(metadataType);
            _logger.LogInformation("Adding singleton handler container {containerType} with {metadataType} to projection manager builder for {managerKey}", classType.Name, metadataType.Name, Key);
            var factory = Services.GetService<IMethodProviderFactory>();
            var provider = factory.GetFuncProvider<IEvent, IEventMetadata, ProjectionEventResult>(_standard);
            var methods = provider.GetAsyncMethods(classType, t1Required: true);

            var instance = ActivatorUtilities.CreateInstance(Services, classType);
            foreach (var method in methods)
            {
                var identifier = GetIdentifier(method);
                var handler = new SingletonProjectionEventHandler(identifier, metadataType, instance, method);

                _logger.LogInformation("Adding singleton handler for {identifier} with meta data type {metadataType} to projection manager builder for {managerKey}", classType.Name, metadataType.Name, Key);
                _registrationMethods.Add(x => x.Register(handler));
            }
        }
        public void AddSingleton<TProjectionContainer, TMetadataType>()
            where TMetadataType : IEventMetadata
        {
            AddSingleton(typeof(TProjectionContainer), typeof(TMetadataType));
        }
        public void AddScoped(Type type, Type metadataType)
        {
            CheckAndThrowMetadataType(metadataType);
            _logger.LogInformation("Adding scoped handler container {containerType} with {metadataType} to projection manager builder for {managerKey}", type.Name, metadataType.Name, Key);
            var factory = Services.GetService<IMethodProviderFactory>();
            var provider = factory.GetFuncProvider<IEvent, IEventMetadata, ProjectionEventResult>(_standard);
            var methods = provider.GetAsyncMethods(type, t1Required: true);

            foreach (var method in methods)
            {
                string identifier = GetIdentifier(method);
                var handler = new ScopedProjectionEventHandler(identifier, metadataType, method);

                _logger.LogInformation("Adding scoped handler for {identifier} with meta data type {metadataType} to projection manager builder for {managerKey}", type.Name, metadataType.Name, Key);
                _registrationMethods.Add(x => x.Register(handler));
            }
        }
        public void AddScoped<TProjectionContainer, TMetadataType>()
            where TMetadataType : IEventMetadata
        {
            AddScoped(typeof(TProjectionContainer), typeof(TMetadataType));
        }
        public void AddAssemblyProjections(Assembly assembly)
        {
            _logger.LogInformation("Adding assembly handlers for {assemblyName} to projection manager builder for {managerKey}", assembly.FullName, Key);
            var assemblyTypes = assembly.GetTypes();
            foreach (var type in assemblyTypes)
            {
                Add(type, false);
            }
        }

        public void AddSingleton(Type projectionContainer)
        {
            var attribute = projectionContainer.GetCustomAttribute<ProjectionHandlerContainerAttribute>();
            if (attribute == null)
            {
                throw new ArgumentException($"{projectionContainer.Name} is missing {nameof(ProjectionEventHandlerAttribute)} please add the attribute to define the metadata type or use an override to define it");
            }

            AddSingleton(projectionContainer, attribute.MetadataType);
        }

        public void AddSingleton<TProjectionContainer>()
        {
            AddSingleton(typeof(TProjectionContainer));
        }

        public void AddScoped(Type projectionContainer)
        {
            var attribute = projectionContainer.GetCustomAttribute<ProjectionHandlerContainerAttribute>();
            if (attribute == null)
            {
                throw new ArgumentException($"{projectionContainer.Name} is missing {nameof(ProjectionEventHandlerAttribute)} please add the attribute to define the metadata type or use an override to define it");
            }
            AddScoped(projectionContainer, attribute.MetadataType);
        }

        public void AddScoped<TProjectionContainer>()
        {
            AddScoped(typeof(TProjectionContainer));
        }
    }
}
