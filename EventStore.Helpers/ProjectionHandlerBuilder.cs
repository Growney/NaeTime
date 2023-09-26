using EventStore.Client;

namespace EventStore.Helpers;
public class ProjectionHandlerBuilder : IProjectionBuilder
{
    private readonly EventStoreClient _client;
    private readonly IServiceProvider _serviceProvider;

    private readonly List<IProjectionHandler> _handlers = new();

    public ProjectionHandlerBuilder(EventStoreClient client, IServiceProvider serviceProvider)
    {
        _client = client;
        _serviceProvider = serviceProvider;
    }

    public void AddAllStreamProjectionHandler(FromAll position, Action<IProjectionHandler> configure)
    {
        var allStreamHandler = new AllStreamProjectionHandler(_serviceProvider, position, _client);

        configure(allStreamHandler);

        _handlers.Add(allStreamHandler);
    }

    public void AddStreamProjectionHandler(string streamName, FromStream position, Action<IProjectionHandler> configure)
    {
        var streamHandler = new StreamProjectionHandler(_serviceProvider, streamName, position, _client);

        configure(streamHandler);

        _handlers.Add(streamHandler);
    }

    public async Task StartAsync()
    {
        foreach (var handler in _handlers)
        {
            await handler.Start();
        }
    }

    public async Task StopAsync()
    {
        foreach (var handler in _handlers)
        {
            await handler.Stop();
        }
    }
}
