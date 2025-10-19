using EventDbLite.Abstractions;
using Microsoft.Extensions.Hosting;

namespace EventDbLite.Projections;

public class LiveProjectionService : IHostedService
{
    private readonly IEventStoreLite _eventStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<LiveProjectionManager> _projections = [];

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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (LiveProjectionManager projection in _projections)
        {
            await projection.Start(cancellationToken);
        }
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (LiveProjectionManager projection in _projections)
        {
            await projection.Stop();
        }
    }
}
