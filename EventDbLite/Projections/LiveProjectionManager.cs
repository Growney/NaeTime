using EventDbLite.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;

namespace EventDbLite.Projections;

internal class LiveProjectionManager
{
    private readonly LiveProjectionRequirement _requirement;
    private readonly IEventStoreLite _eventStore;
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource? _cancellationTokenSource;

    public LiveProjectionManager(LiveProjectionRequirement requirement, IServiceProvider serviceProvider, IEventStoreLite eventStore)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _requirement = requirement ?? throw new ArgumentNullException(nameof(requirement));
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
    }

    public async Task Start()
    {
        LiveProjection initialInstance = GetInstance(_serviceProvider);
        StreamPosition initialPosition = await initialInstance.GetGlobalPosition();

        IStreamSubscription subscription = _requirement.Stream is not null
            ? _eventStore.SubscribeToStream(_requirement.Stream, initialPosition)
            : _eventStore.SubscribeToAllStreams(initialPosition);

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        while (_cancellationTokenSource.IsCancellationRequested)
        {
            StreamEvent? nextEvent = await subscription.WaitForNextEvent(_cancellationTokenSource.Token);
            if (nextEvent is null)
            {
                continue; // No event received, continue to wait
            }

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

    public void Stop()
    {
        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

}
