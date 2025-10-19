using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventDbLite.Projections;

public class LiveProjectionService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<LiveProjectionManager> _projections = [];

    public LiveProjectionService(IServiceProvider serviceProvider, IEnumerable<LiveProjectionRequirement> projections)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        foreach (LiveProjectionRequirement requirement in projections)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            LiveProjectionManager projectionManager = ActivatorUtilities.CreateInstance<LiveProjectionManager>(scope.ServiceProvider, requirement);
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
