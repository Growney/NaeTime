using Microsoft.Extensions.Logging;
using NodaTime.TimeZones;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Core.Reflection.Abstractions;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Aggregate.Core;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Event.Core;

namespace EventSourcingCore.Aggregate.Core
{
    public class StandardAggregateEventHandlerRepository : IAggregateEventHandlerRepository
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, IAggregateEventHandler>> _repo = new ConcurrentDictionary<Type, ConcurrentDictionary<string, IAggregateEventHandler>>();
        private readonly MethodStandard _aggregateStandard = new MethodStandard(
            name: "When",
            shouldWrapSynchronousMethods: false,
            allowDiscardedReturnTypes: false,
            overrideAttributes: new Type[] { typeof(EventHandlerAttribute) });

        private readonly IStandardActionProvider<IEvent> _provider;
        private readonly ILogger<StandardAggregateEventHandlerRepository> _logger;

        public StandardAggregateEventHandlerRepository(IMethodProviderFactory factory, ILogger<StandardAggregateEventHandlerRepository> logger)
        {
            _provider = factory.GetActionProvider<IEvent>(_aggregateStandard);
            _logger = logger;
        }
        public StandardAggregateEventHandlerRepository(IMethodProviderFactory factory)
            : this(factory, null)
        {

        }
        public IAggregateEventHandler GetHandler(Type aggregateType, string identifier)
        {
            _logger?.LogTrace("Getting handler from {rootType} with {identifier}", aggregateType.Name, identifier);
            if (!_repo.ContainsKey(aggregateType))
            {
                RegisterAggregate(aggregateType);
            }
            if (_repo.TryGetValue(aggregateType, out var typeHandlers))
            {
                if (typeHandlers.TryGetValue(identifier, out var handler))
                {
                    return handler;
                }
                else
                {
                    _logger?.LogWarning("No handler found with {identifier} in {rootType}", identifier, aggregateType.Name);
                }
            }
            else
            {
                _logger?.LogWarning("No handlers found for {rootType}", aggregateType.Name);
            }
            return null;
        }

        private string GetIdentifier(IStandardActionMethod<IEvent> method)
        {
            foreach (var attribute in method.Attributes)
            {
                if (attribute is EventHandlerAttribute handlerAttribute)
                {
                    if (!string.IsNullOrWhiteSpace(handlerAttribute.Identifier))
                    {
                        return handlerAttribute.Identifier;
                    }
                }
            }

            return method.T1Type.GetEventIdentifier();
        }

        public void RegisterAggregate(Type aggregateType)
        {
            _logger?.LogTrace("Registering aggregate {rootType}", aggregateType.Name);
            var handlers = _provider.GetMethods(aggregateType, t1Required: true);

            var handlerRepo = _repo.GetOrAdd(aggregateType, x =>
             {
                 return new ConcurrentDictionary<string, IAggregateEventHandler>();
             });

            foreach (var handler in handlers)
            {
                var identifier = GetIdentifier(handler);
                _logger?.LogInformation("Registering standard handler for {identifier} in {rootType}", identifier, aggregateType.Name);
                handlerRepo.TryAdd(identifier, new StandardAggregateEventHandler(identifier, handler.T1Type, handler));
            }
        }

    }
}
