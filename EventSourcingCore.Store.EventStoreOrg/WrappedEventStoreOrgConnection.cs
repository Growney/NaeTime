using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Event.Abstractions;
using EventData = EventStore.ClientAPI.EventData;

namespace EventSourcingCore.Store.EventStoreOrg
{
    public class WrappedEventStoreOrgConnection : IEventStoreConnection, IEventStoreConnectionWithStatus
    {

        [Flags]
        private enum Status
        {
            Connecting = 0x00,
            AuthenticationFailed = 0x01,
            ErrorOccurred = 0x04,
            Disconnected = 0x08,
            Connected = 0x10,
            SocketConnectionFailure = 0x20,
            ConnectionDisposed = 0x40,
        }

        private readonly bool _useConnectionString = false;
        private readonly string _connectionString;
        private readonly ConnectionSettings _settings;
        private readonly Uri _endPoint;
        private readonly string _connectionName;


        private Status _currentStatus = Status.Connecting;

        private IEventStoreConnection _connection;

        private readonly object _connectLock = new object();
        private Task _connectionTask;

        public WrappedEventStoreOrgConnection(ConnectionSettings settings, Uri endPoint, string connectionName)
        {
            _settings = settings;
            _endPoint = endPoint;
            _connectionName = connectionName;
        }

        public WrappedEventStoreOrgConnection(string connectionString)
        {
            _useConnectionString = true;
            _connectionString = connectionString;

        }
        private void Connection_AuthenticationFailed(object sender, ClientAuthenticationFailedEventArgs e)
        {
            _currentStatus |= Status.AuthenticationFailed;
        }
        private void Connection_Reconnecting(object sender, ClientReconnectingEventArgs e)
        {
            _currentStatus = Status.Connecting;
        }
        private void Connection_ErrorOccurred(object sender, ClientErrorEventArgs e)
        {
            _currentStatus |= Status.ErrorOccurred;
        }
        private void Connection_Disconnected(object sender, ClientConnectionEventArgs e)
        {
            _currentStatus |= Status.Disconnected;
        }
        private void Connection_Connected(object sender, ClientConnectionEventArgs e)
        {
            _currentStatus |= Status.Connected;
        }

        public bool IsConnected => _currentStatus == Status.Connected;
        public string ConnectionStatus => _currentStatus.ToString();

        #region --- Direct Passthrough ---
        public string ConnectionName => _connection.ConnectionName;
        public ConnectionSettings Settings => _connection.Settings;

        public event EventHandler<ClientConnectionEventArgs> Connected { add => _connection.Connected += value; remove => _connection.Connected -= value; }
        public event EventHandler<ClientConnectionEventArgs> Disconnected { add => _connection.Disconnected += value; remove => _connection.Disconnected -= value; }
        public event EventHandler<ClientReconnectingEventArgs> Reconnecting { add => _connection.Reconnecting += value; remove => _connection.Reconnecting -= value; }
        public event EventHandler<ClientClosedEventArgs> Closed { add => _connection.Closed += value; remove => _connection.Closed -= value; }
        public event EventHandler<ClientErrorEventArgs> ErrorOccurred { add => _connection.ErrorOccurred += value; remove => _connection.ErrorOccurred -= value; }
        public event EventHandler<ClientAuthenticationFailedEventArgs> AuthenticationFailed { add => _connection.AuthenticationFailed += value; remove => _connection.AuthenticationFailed -= value; }
        public Task<WriteResult> AppendToStreamAsync(string stream, long expectedVersion, params EventData[] events)
            => _connection.AppendToStreamAsync(stream, expectedVersion, events);
        public Task<WriteResult> AppendToStreamAsync(string stream, long expectedVersion, UserCredentials userCredentials, params EventData[] events)
            => _connection.AppendToStreamAsync(stream, expectedVersion, userCredentials, events);
        public Task<WriteResult> AppendToStreamAsync(string stream, long expectedVersion, IEnumerable<EventData> events, UserCredentials userCredentials = null)
            => _connection.AppendToStreamAsync(stream, expectedVersion, events, userCredentials);
        public Task<ConditionalWriteResult> ConditionalAppendToStreamAsync(string stream, long expectedVersion, IEnumerable<EventData> events, UserCredentials userCredentials = null)
            => _connection.ConditionalAppendToStreamAsync(stream, expectedVersion, events, userCredentials);
        public EventStorePersistentSubscriptionBase ConnectToPersistentSubscription(string stream, string groupName, Func<EventStorePersistentSubscriptionBase, ResolvedEvent, int?, Task> eventAppeared, Action<EventStorePersistentSubscriptionBase, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null, int bufferSize = 10, bool autoAck = true)
            => _connection.ConnectToPersistentSubscription(stream, groupName, eventAppeared, subscriptionDropped, userCredentials, bufferSize, autoAck);
        public Task<EventStorePersistentSubscriptionBase> ConnectToPersistentSubscriptionAsync(string stream, string groupName, Func<EventStorePersistentSubscriptionBase, ResolvedEvent, int?, Task> eventAppeared, Action<EventStorePersistentSubscriptionBase, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null, int bufferSize = 10, bool autoAck = true)
            => _connection.ConnectToPersistentSubscriptionAsync(stream, groupName, eventAppeared, subscriptionDropped, userCredentials, bufferSize, autoAck);
        public EventStoreTransaction ContinueTransaction(long transactionId, UserCredentials userCredentials = null)
        => _connection.ContinueTransaction(transactionId, userCredentials);
        public Task CreatePersistentSubscriptionAsync(string stream, string groupName, PersistentSubscriptionSettings settings, UserCredentials credentials)
        => _connection.CreatePersistentSubscriptionAsync(stream, groupName, settings, credentials);
        public Task DeletePersistentSubscriptionAsync(string stream, string groupName, UserCredentials userCredentials = null)
        => _connection.DeletePersistentSubscriptionAsync(stream, groupName, userCredentials);
        public Task<DeleteResult> DeleteStreamAsync(string stream, long expectedVersion, UserCredentials userCredentials = null)
        => _connection.DeleteStreamAsync(stream, expectedVersion, userCredentials);
        public Task<DeleteResult> DeleteStreamAsync(string stream, long expectedVersion, bool hardDelete, UserCredentials userCredentials = null)
        => _connection.DeleteStreamAsync(stream, expectedVersion, hardDelete, userCredentials);
        public Task<RawStreamMetadataResult> GetStreamMetadataAsRawBytesAsync(string stream, UserCredentials userCredentials = null)
        => _connection.GetStreamMetadataAsRawBytesAsync(stream, userCredentials);
        public Task<StreamMetadataResult> GetStreamMetadataAsync(string stream, UserCredentials userCredentials = null)
        => _connection.GetStreamMetadataAsync(stream, userCredentials);
        public Task<AllEventsSlice> ReadAllEventsBackwardAsync(Position position, int maxCount, bool resolveLinkTos, UserCredentials userCredentials = null)
        => _connection.ReadAllEventsBackwardAsync(position, maxCount, resolveLinkTos, userCredentials);
        public Task<AllEventsSlice> ReadAllEventsForwardAsync(Position position, int maxCount, bool resolveLinkTos, UserCredentials userCredentials = null)
        => _connection.ReadAllEventsForwardAsync(position, maxCount, resolveLinkTos, userCredentials);
        public Task<EventReadResult> ReadEventAsync(string stream, long eventNumber, bool resolveLinkTos, UserCredentials userCredentials = null)
        => _connection.ReadEventAsync(stream, eventNumber, resolveLinkTos, userCredentials);
        public Task<StreamEventsSlice> ReadStreamEventsBackwardAsync(string stream, long start, int count, bool resolveLinkTos, UserCredentials userCredentials = null)
        => _connection.ReadStreamEventsBackwardAsync(stream, start, count, resolveLinkTos, userCredentials);
        public Task<StreamEventsSlice> ReadStreamEventsForwardAsync(string stream, long start, int count, bool resolveLinkTos, UserCredentials userCredentials = null)
        => _connection.ReadStreamEventsForwardAsync(stream, start, count, resolveLinkTos, userCredentials);
        public Task<WriteResult> SetStreamMetadataAsync(string stream, long expectedMetastreamVersion, StreamMetadata metadata, UserCredentials userCredentials = null)
        => _connection.SetStreamMetadataAsync(stream, expectedMetastreamVersion, metadata, userCredentials);
        public Task<WriteResult> SetStreamMetadataAsync(string stream, long expectedMetastreamVersion, byte[] metadata, UserCredentials userCredentials = null)
        => _connection.SetStreamMetadataAsync(stream, expectedMetastreamVersion, metadata, userCredentials);
        public Task SetSystemSettingsAsync(SystemSettings settings, UserCredentials userCredentials = null)
        => _connection.SetSystemSettingsAsync(settings, userCredentials);
        public Task<EventStoreTransaction> StartTransactionAsync(string stream, long expectedVersion, UserCredentials userCredentials = null)
        => _connection.StartTransactionAsync(stream, expectedVersion, userCredentials);
        public Task<EventStoreSubscription> SubscribeToAllAsync(bool resolveLinkTos, Func<EventStoreSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null)
        => _connection.SubscribeToAllAsync(resolveLinkTos, eventAppeared, subscriptionDropped, userCredentials);
        public EventStoreAllCatchUpSubscription SubscribeToAllFrom(Position? lastCheckpoint, CatchUpSubscriptionSettings settings, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreCatchUpSubscription> liveProcessingStarted = null, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null)
        => _connection.SubscribeToAllFrom(lastCheckpoint, settings, eventAppeared, liveProcessingStarted, subscriptionDropped, userCredentials);
        public Task<EventStoreSubscription> SubscribeToStreamAsync(string stream, bool resolveLinkTos, Func<EventStoreSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null)
        => _connection.SubscribeToStreamAsync(stream, resolveLinkTos, eventAppeared, subscriptionDropped, userCredentials);
        public EventStoreStreamCatchUpSubscription SubscribeToStreamFrom(string stream, long? lastCheckpoint, CatchUpSubscriptionSettings settings, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreCatchUpSubscription> liveProcessingStarted = null, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null)
        => _connection.SubscribeToStreamFrom(stream, lastCheckpoint, settings, eventAppeared, liveProcessingStarted, subscriptionDropped, userCredentials);
        public Task UpdatePersistentSubscriptionAsync(string stream, string groupName, PersistentSubscriptionSettings settings, UserCredentials credentials)
        => _connection.UpdatePersistentSubscriptionAsync(stream, groupName, settings, credentials);

        public Task<AllEventsSlice> FilteredReadAllEventsForwardAsync(Position position, int maxCount, bool resolveLinkTos, Filter filter, UserCredentials userCredentials = null)
        {
            return _connection.FilteredReadAllEventsForwardAsync(position, maxCount, resolveLinkTos, filter, userCredentials);
        }

        public Task<AllEventsSlice> FilteredReadAllEventsForwardAsync(Position position, int maxCount, bool resolveLinkTos, Filter filter, int maxSearchWindow, UserCredentials userCredentials = null)
        {
            return _connection.FilteredReadAllEventsForwardAsync(position, maxCount, resolveLinkTos, filter, maxSearchWindow, userCredentials);
        }

        public Task<AllEventsSlice> FilteredReadAllEventsBackwardAsync(Position position, int maxCount, bool resolveLinkTos, Filter filter, int maxSearchWindow, UserCredentials userCredentials = null)
        {
            return _connection.FilteredReadAllEventsBackwardAsync(position, maxCount, resolveLinkTos, filter, maxSearchWindow, userCredentials);
        }

        public Task<AllEventsSlice> FilteredReadAllEventsBackwardAsync(Position position, int maxCount, bool resolveLinkTos, Filter filter, UserCredentials userCredentials = null)
        {
            return _connection.FilteredReadAllEventsBackwardAsync(position, maxCount, resolveLinkTos, filter, userCredentials);
        }

        public Task<EventStoreSubscription> FilteredSubscribeToAllAsync(bool resolveLinkTos, Filter filter, Func<EventStoreSubscription, ResolvedEvent, Task> eventAppeared, Func<EventStoreSubscription, Position, Task> checkpointReached, int checkpointInterval, Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null)
        {
            return _connection.FilteredSubscribeToAllAsync(resolveLinkTos, filter, eventAppeared, checkpointReached, checkpointInterval, subscriptionDropped, userCredentials);
        }

        public Task<EventStoreSubscription> FilteredSubscribeToAllAsync(bool resolveLinkTos, Filter filter, Func<EventStoreSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null)
        {
            return _connection.FilteredSubscribeToAllAsync(resolveLinkTos, filter, eventAppeared, subscriptionDropped, userCredentials);
        }

        public EventStoreAllFilteredCatchUpSubscription FilteredSubscribeToAllFrom(Position? lastCheckpoint, Filter filter, CatchUpSubscriptionFilteredSettings settings, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> eventAppeared, Func<EventStoreCatchUpSubscription, Position, Task> checkpointReached, int checkpointIntervalMultiplier, Action<EventStoreCatchUpSubscription> liveProcessingStarted = null, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null)
        {
            return _connection.FilteredSubscribeToAllFrom(lastCheckpoint, filter, settings, eventAppeared, checkpointReached, checkpointIntervalMultiplier, liveProcessingStarted, subscriptionDropped, userCredentials);
        }

        public EventStoreAllFilteredCatchUpSubscription FilteredSubscribeToAllFrom(Position? lastCheckpoint, Filter filter, CatchUpSubscriptionFilteredSettings settings, Func<EventStoreCatchUpSubscription, ResolvedEvent, Task> eventAppeared, Action<EventStoreCatchUpSubscription> liveProcessingStarted = null, Action<EventStoreCatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null, UserCredentials userCredentials = null)
        {
            return _connection.FilteredSubscribeToAllFrom(lastCheckpoint, filter, settings, eventAppeared, liveProcessingStarted, subscriptionDropped, userCredentials);
        }
        #endregion --- Direct Passthrough ---

        private Task GetConnectionTask()
        {
            lock (_connectLock)
            {
                if (_connectionTask == null)
                {
                    try
                    {
                        _connection = CreateConnection();

                        _connection.Connected += Connection_Connected;
                        _connection.Disconnected += Connection_Disconnected;
                        _connection.ErrorOccurred += Connection_ErrorOccurred;
                        _connection.Reconnecting += Connection_Reconnecting;
                        _connection.AuthenticationFailed += Connection_AuthenticationFailed;

                        _connectionTask = _connection.ConnectAsync();
                        _currentStatus = Status.Connecting;

                    }
                    catch (Exception ex)
                    {
                        _connectionTask = Task.CompletedTask;
                        throw ex;
                    }
                }
            }
            return _connectionTask;
        }

        private IEventStoreConnection CreateConnection()
        {
            if (_useConnectionString)
            {
                return EventStoreConnection.Create(_connectionString);
            }
            else
            {
                return EventStoreConnection.Create(_settings, _endPoint, _connectionName);
            }
        }
        public async Task<bool> ConnectWithStatusAsync()
        {
            await ConnectAsync();
            return _currentStatus == Status.Connected || _currentStatus == Status.Connecting;
        }
        public async Task ConnectAsync()
        {
            try
            {
                await GetConnectionTask();

            }
            catch (SocketException)
            {
                _currentStatus |= Status.SocketConnectionFailure;
            }
            catch (ObjectDisposedException)
            {
                _currentStatus |= Status.ConnectionDisposed;
            }
            catch (InvalidOperationException)
            {
                //Ignore invalid operation as it means it is connecting or attempt to connect
            }

        }
        public void Dispose()
        {
            lock (_connectLock)
            {
                _connectionTask = null;
            }
            _connection?.Dispose();
        }
        public void Close()
        {
            lock (_connectLock)
            {
                _connectionTask = null;
            }
            _connection?.Close();
        }


    }
}
