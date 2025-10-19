using EventDbLite.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;

namespace EventDbLite.Projections;

internal class LiveProjectionManager : IAsyncDisposable
{
    private readonly LiveProjectionRequirement _requirement;
    private readonly IEventStoreLite _eventStore;
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task _continueTask = Task.CompletedTask;

    public LiveProjectionManager(LiveProjectionRequirement requirement, IServiceProvider serviceProvider, IEventStoreLite eventStore)
    {
        _requirement = requirement ?? throw new ArgumentNullException(nameof(requirement));
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task Start(CancellationToken token)
    {
        using IServiceScope initialScope = _serviceProvider.CreateScope();
        LiveProjection initialInstance = GetInstance(initialScope.ServiceProvider);
        StreamPosition initialPosition = await initialInstance.GetGlobalPosition();

        IStreamSubscription subscription = _requirement.Stream is not null
            ? _eventStore.SubscribeToStream(_requirement.Stream, initialPosition)
            : _eventStore.SubscribeToAllStreams(initialPosition);

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

        await foreach (SubscriptionEvent nextEvent in subscription.CatchUp(_cancellationTokenSource.Token))
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            LiveProjection projection = GetInstance(scope.ServiceProvider);
            await projection.Raise(nextEvent);
        }

        _continueTask = ContinueMonitoring(subscription, _cancellationTokenSource.Token);
    }

    private async Task ContinueMonitoring(IStreamSubscription subscription, CancellationToken token)
    {
        await foreach (SubscriptionEvent nextEvent in subscription.CatchUp(token))
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            LiveProjection projection = GetInstance(scope.ServiceProvider);
            await projection.Raise(nextEvent);
        }
    }

    private LiveProjection GetInstance(IServiceProvider serviceProvider)
    {
        LiveProjection projection = (LiveProjection)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, _requirement.ProjectionType);
        return projection;
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
