using EventDbLite.Abstractions;
using Microsoft.Extensions.Hosting;

namespace EventDbLite.Projections;

public class LiveProjectionService : IHostedService
{
    private readonly IEventStoreLite _eventStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<LiveProjectionManager> _projections = new();

    public LiveProjectionService(IEventStoreLite eventStore, IServiceProvider serviceProvider, IEnumerable<LiveProjectionRequirement> projections)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        foreach (LiveProjectionRequirement requirement in projections)
        {
            LiveProjectionManager projectionManager = new(requirement, _serviceProvider, _eventStore);
            _projections.Add(projectionManager);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (LiveProjectionManager projection in _projections)
        {
            _ = projection.Start();
        }

        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (LiveProjectionManager projection in _projections)
        {
            projection.Stop();
        }

        return Task.CompletedTask;
    }
}
