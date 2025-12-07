using EventDbLite.Abstractions;
using EventDbLite.Handlers;
using EventDbLite.Handlers.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace EventDbLite.Projections;

internal class LiveProjectionManager : IAsyncDisposable, ILiveProjectionManager
{
    private readonly LiveProjectionRequirement _requirement;
    private readonly IEventStoreLite _eventStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventSerializer _serializer;
    private readonly IAsyncHandlerProvider _asyncHandlerProvider;

    private CancellationTokenSource? _cancellationTokenSource;
    private Task _continueTask = Task.CompletedTask;

    private class VersionWaiter
    {
        public long GlobalPosition { get; }
        public TaskCompletionSource CompletionSource { get; }
        public VersionWaiter(long globalPosition)
        {
            GlobalPosition = globalPosition;
            CompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    private long _currentGlobalPosition = -1;

    private ConcurrentBag<VersionWaiter> _waitingTasks = new();
    public LiveProjectionManager(IServiceProvider serviceProvider, IEventSerializer serializer, IAsyncHandlerProvider asyncHandlerProvider, IEventStoreLite eventStore, LiveProjectionRequirement requirement)
    {
        _requirement = requirement ?? throw new ArgumentNullException(nameof(requirement));
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _asyncHandlerProvider = asyncHandlerProvider ?? throw new ArgumentNullException(nameof(asyncHandlerProvider));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task Start(CancellationToken token)
    {
        using IServiceScope initialScope = _serviceProvider.CreateScope();
        IEventSerializer eventSerializer = initialScope.ServiceProvider.GetRequiredService<IEventSerializer>();

        //Later if there is a requirement we can load the projection position using reflection
        StreamPosition initialPosition = StreamPosition.Beginning;

        IStreamSubscription subscription = _requirement.Stream is not null
            ? _eventStore.SubscribeToStream(_requirement.Stream, initialPosition)
            : _eventStore.SubscribeToAllStreams(initialPosition);

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        await foreach (SubscriptionEvent nextEvent in subscription.CatchUp(_cancellationTokenSource.Token))
        {
            await RaiseProjectionEvent(nextEvent);
            NotifyWaitingTasks(nextEvent.Event.GlobalOrdinal);
        }

        _continueTask = ContinueMonitoring(subscription, _cancellationTokenSource.Token);
    }

    public Task WaitForVersion(long globalPosition, CancellationToken cancellationToken)
    {
        if (_currentGlobalPosition >= globalPosition)
        {
            return Task.CompletedTask;
        }

        VersionWaiter waiter = new(globalPosition);
        _waitingTasks.Add(waiter);
        cancellationToken.Register(() => waiter.CompletionSource.TrySetCanceled(cancellationToken));
        return waiter.CompletionSource.Task;
    }
    private void NotifyWaitingTasks(long globalPosition)
    {
        foreach (VersionWaiter waiter in _waitingTasks)
        {
            if (waiter.GlobalPosition <= globalPosition)
            {
                waiter.CompletionSource.TrySetResult();
                _waitingTasks.TryTake(out _);
            }
        }
    }
    private async Task ContinueMonitoring(IStreamSubscription subscription, CancellationToken token)
    {
        await foreach (SubscriptionEvent nextEvent in subscription.StreamEvents(token))
        {
            await RaiseProjectionEvent(nextEvent);
            _currentGlobalPosition = nextEvent.Event.GlobalOrdinal;
            NotifyWaitingTasks(_currentGlobalPosition);
        }
    }
    private async Task RaiseProjectionEvent(SubscriptionEvent subscriptionEvent)
    {
        EventMetadata metadata = _serializer.DeserializeMetadata(subscriptionEvent.Event.Data.Metadata);
        using IServiceScope scope = _serviceProvider.CreateScope();
        object? projection = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, _requirement.ProjectionType);
        AsyncHandler? handler = _asyncHandlerProvider.GetHandlerMethod(projection.GetType(), metadata.Identifier);

        if (handler is null)
        {
            return;
        }

        object? payload = _serializer.DeserializeEvent(subscriptionEvent.Event.Data.Payload, handler.TargetType)
            ?? throw new InvalidOperationException($"Failed to deserialize event payload for identifier '{metadata.Identifier}'");

        await handler.Action(projection, payload);
    }

    public async ValueTask Stop()
    {
        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        await _continueTask;
    }

    public ValueTask DisposeAsync() => Stop();

}
