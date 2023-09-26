using Microsoft.Extensions.Hosting;

namespace EventStore.Helpers;
public class ProjectionService : IHostedService
{
    private readonly ProjectionHandlerBuilder _handlerBuilder;

    public ProjectionService(ProjectionHandlerBuilder handlerBuilder)
    {
        _handlerBuilder = handlerBuilder;
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => _handlerBuilder.StartAsync();

    public Task StopAsync(CancellationToken cancellationToken)
        => _handlerBuilder.StopAsync();
}
