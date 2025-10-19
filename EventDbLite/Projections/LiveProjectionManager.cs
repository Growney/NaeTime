using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Handlers;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;

namespace EventDbLite.Projections;

internal class LiveProjectionManager : IAsyncDisposable
{
    private readonly LiveProjectionRequirement _requirement;
    private readonly IEventStoreLite _eventStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventSerializer _serializer;
    private readonly IAsyncHandlerProvider _asyncHandlerProvider;

    private CancellationTokenSource? _cancellationTokenSource;
    private Task _continueTask = Task.CompletedTask;

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
        long finalPosition = 0;
        await foreach (SubscriptionEvent nextEvent in subscription.CatchUp(_cancellationTokenSource.Token))
        {
            await RaiseProjectionEvent(nextEvent);
            finalPosition = nextEvent.Event.GlobalOrdinal;
        }

        _continueTask = ContinueMonitoring(subscription, _cancellationTokenSource.Token);
    }

    private async Task ContinueMonitoring(IStreamSubscription subscription, CancellationToken token)
    {
        await foreach (SubscriptionEvent nextEvent in subscription.StreamEvents(token))
        {
            await RaiseProjectionEvent(nextEvent);
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
