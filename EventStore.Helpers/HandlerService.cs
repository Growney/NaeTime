using Microsoft.Extensions.Hosting;

namespace EventStore.Helpers;
internal class HandlerService : IHostedService
{
    private readonly IHandlerProvider _handlerProvider;

    public HandlerService(IHandlerProvider handlerProvider)
    {
        _handlerProvider = handlerProvider ?? throw new ArgumentNullException(nameof(handlerProvider));
    }

    public Task StartAsync(CancellationToken cancellationToken) => _handlerProvider.Start();
    public Task StopAsync(CancellationToken cancellationToken) => _handlerProvider.Stop();
}
