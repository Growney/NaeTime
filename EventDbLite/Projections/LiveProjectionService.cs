using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventDbLite.Projections;

public class LiveProjectionService : IHostedService, IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly List<LiveProjectionManager> _projections = [];

    public LiveProjectionService(IServiceProvider serviceProvider, IEnumerable<LiveProjectionRequirement> projections)
    {
        _serviceScope = serviceProvider.CreateScope();

        foreach (LiveProjectionRequirement requirement in projections)
        {
            LiveProjectionManager projectionManager = ActivatorUtilities.CreateInstance<LiveProjectionManager>(_serviceScope.ServiceProvider, requirement);
            _projections.Add(projectionManager);
        }
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
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
