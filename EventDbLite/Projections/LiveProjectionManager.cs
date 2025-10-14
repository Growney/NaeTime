using EventDbLite.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;

namespace EventDbLite.Projections;

internal class LiveProjectionManager(LiveProjectionRequirement requirement, IServiceProvider serviceProvider, IEventStoreLite eventStore)
{
    private readonly LiveProjectionRequirement _requirement = requirement ?? throw new ArgumentNullException(nameof(requirement));
    private readonly IEventStoreLite _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private CancellationTokenSource? _cancellationTokenSource;

    public async Task Start()
    {
        using IServiceScope initialScope = _serviceProvider.CreateScope();
        LiveProjection initialInstance = GetInstance(initialScope.ServiceProvider);
        StreamPosition initialPosition = await initialInstance.GetGlobalPosition();

        IStreamSubscription subscription = _requirement.Stream is not null
            ? _eventStore.SubscribeToStream(_requirement.Stream, initialPosition)
            : _eventStore.SubscribeToAllStreams(initialPosition);

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        await foreach (StreamEvent nextEvent in subscription.StreamEvents(_cancellationTokenSource.Token))
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
