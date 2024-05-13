using Microsoft.Extensions.Hosting;

namespace NaeTime.Client.MAUI.Lib;
public class ServiceRunner : IAsyncDisposable
{
    private readonly IEnumerable<IHostedService> _services;

    public ServiceRunner(IEnumerable<IHostedService> services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public async ValueTask DisposeAsync()
    {
        foreach (IHostedService service in _services)
        {
            await service.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    public async Task Start()
    {
        foreach (IHostedService service in _services)
        {
            await service.StartAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

}
