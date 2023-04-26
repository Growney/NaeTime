using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Security.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Projection.Abstractions;
using EventSourcingCore.Store.Abstractions;
using Core.Collections;

namespace EventSourcingCore.Projection.Core
{
    public class ProjectionManager : IProjectionManager
    {
        public string Key { get; }
        private readonly IEventStoreSubscriptionConnection _connection;
        private readonly IEventFactory _factory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IProjectionPositionRepository _positionRepository;
        private readonly ILogger _logger;
        public bool StreamMode => !string.IsNullOrWhiteSpace(StreamName);
        public string StreamName { get; }


        private ConcurrentDictionary<string, Func<ReadEventData, Task<ProjectionEventResult>>> _handlers = new ConcurrentDictionary<string, Func<ReadEventData, Task<ProjectionEventResult>>>();
        public ProjectionManager(string key,
            string streamName,
            IEventStoreSubscriptionConnection connection,
            IEventFactory factory,
            IServiceProvider provider,
            IProjectionPositionRepository positionRepository,
            ILogger<ProjectionManager> logger)
        {
            Key = key;
            StreamName = streamName;

            _logger = logger;
            _factory = factory;
            _connection = connection;
            _serviceProvider = provider;
            _positionRepository = positionRepository;
        }
        public ProjectionManager(string key,
            IEventStoreSubscriptionConnection connection,
            IEventFactory factory,
            IServiceProvider provider,
            IProjectionPositionRepository positionRepository,
            ILogger<ProjectionManager> logger)
        {
            Key = key;

            _logger = logger;
            _factory = factory;
            _connection = connection;
            _serviceProvider = provider;
            _positionRepository = positionRepository;

        }
        public void Register(IProjectionEventHandler handler)
        {
            _logger.LogInformation("Adding projection handler for {identifier}", handler.Identifier);
            if (!_handlers.TryAdd(handler.Identifier, CreateHandler(handler)))
            {
                throw new InvalidOperationException("Identifier already handled");
            }
        }

        private IDisposable SetupScope(IServiceProvider provider, IEventMetadata metadata)
        {
            var disposableItems = new List<IDisposable>();
            if (metadata != null)
            {
                switch (metadata)
                {
                    case CustomerEventMetadata customerData:
                        {
                            var userContextAccessor = provider.GetService<IUserContextAccessor>();
                            if (userContextAccessor != null)
                            {
                                _logger.LogTrace("Setting projection scope with user id {userId}", customerData.UserID);
                                userContextAccessor.Context = new UserContext(customerData.UserID);

                                disposableItems.Add(provider.GetRequiredService<ILogger<ProjectionManager>>().BeginScope("UserId: {userID}", customerData.UserID));
                            }
                            var customerContextAccessor = provider.GetService<ICustomerContextAccessor>();
                            if (customerContextAccessor != null)
                            {
                                _logger.LogTrace("Setting projection scope with customer id {customerId}", customerData.CustomerID);
                                customerContextAccessor.Context = new CustomerContext(customerData.CustomerID);

                                disposableItems.Add(provider.GetRequiredService<ILogger<ProjectionManager>>().BeginScope("CustomerId: {customerID}", customerData.CustomerID));
                            }
                        }
                        break;
                    case SystemEventMetadata systemData:
                        {
                            var systemContextAccessor = provider.GetService<ISystemContextAccessor>();
                            if (systemContextAccessor != null)
                            {
                                _logger.LogTrace("Setting projection scope with system id {systemId}", systemData.TokenID);
                                systemContextAccessor.Context = new SystemContext(systemData.TokenID);

                                disposableItems.Add(provider.GetRequiredService<ILogger<ProjectionManager>>().BeginScope("TokenID: {tokenId}", systemData.TokenID));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                _logger.LogWarning("No meta data provided to set service provider scope");
            }

            return new DisposableEnumerable<IDisposable>(disposableItems);

        }

        private Func<ReadEventData, Task<ProjectionEventResult>> CreateHandler(IProjectionEventHandler handler)
        {
            return async (data) =>
            {
                _factory.TryCreateMetadata(handler.MetadataType, data.Metadata, out var metadata);

                if (handler.MetadataType != null && metadata == null)
                {
                    _logger.LogWarning("Failed to create {metadataType} for projection handler for {identifier}", handler.MetadataType.Name, handler.Identifier);
                    return ProjectionEventResult.FromMessage($"Unable to parse metadata data with type {handler.MetadataType}", NakAction.Park);
                }

                if (!_factory.TryCreateEvent(handler.EventType, data.Data, out var eventObj))
                {
                    _logger.LogWarning("Failed to create {eventType} for projection handler for {identifier}", handler.EventType.Name, handler.Identifier);
                    return ProjectionEventResult.FromMessage($"Unable to parse event with type {handler.EventType}", NakAction.Park);
                }

                var scope = _serviceProvider.CreateScope();

                var variableScope = SetupScope(scope.ServiceProvider, metadata);

                ProjectionEventResult result;
                try
                {
                    result = await handler.Invoke(scope.ServiceProvider, eventObj, metadata);
                }
                finally
                {
                    variableScope.Dispose();
                    scope.Dispose();
                }
                return result;
            };
        }
        public async Task<bool> Start()
        {
            if (StreamMode)
            {
                if (_positionRepository != null)
                {
                    _logger.LogInformation("Starting projection manager {managerKey} in stream mode using the position repository", Key);
                    return await _connection.SubscribeToStreamAsync(StreamName, EventAppeared(), () => _positionRepository.GetStreamPosition(Key, StreamName));
                }
                else
                {
                    _logger.LogInformation("Starting projection manager {managerKey} in stream mode with no position persistance", Key);
                    return await _connection.SubscribeToStreamAsync(StreamName, EventAppeared());
                }
            }
            else
            {
                if (_positionRepository != null)
                {
                    _logger.LogInformation("Starting projection manager {managerKey} in all mode using the position repository", Key);
                    return await _connection.SubscribeToAllAsync(EventAppeared(), () => _positionRepository.GetStorePosition(Key));
                }
                else
                {
                    _logger.LogInformation("Starting projection manager {managerKey} in all mode with no position persistance", Key);
                    return await _connection.SubscribeToAllAsync(EventAppeared());
                }
            }

        }
        private Func<ReadEventData, Task> EventAppeared() =>
            async (data) =>
            {
                _logger.LogTrace("{identifier} appeared at projection manager {managerKey}", data.EventType, Key);
                if (_handlers.TryGetValue(data.EventType, out var handler))
                {
                    try
                    {
                        _logger.LogInformation("Projection manager {managerKey} handling {identifier}", Key, data.EventType);
                        var res = await handler(data);
                        _logger.LogInformation("Projection manager {managerKey} handled {identifier}", Key, data.EventType);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occured handling event {identifier} in projection manager {managerKey}", data.EventType, Key);
                    }
                }

                if (_positionRepository != null)
                {
                    if (StreamMode)
                    {
                        _logger.LogInformation("Projection manager {managerKey} updaing stream {streamName} position to {streamPosition}", Key, StreamName, data.StreamPosition);
                        await _positionRepository.SetStreamPosition(Key, StreamName, data.StreamPosition);
                    }
                    else
                    {
                        _logger.LogInformation("Projection manager {managerKey} store position to {storePosition}", Key, StreamName, data.StorePosition);
                        await _positionRepository.SetStorePosition(Key, data.StorePosition);
                    }

                }
            };
    }
}
