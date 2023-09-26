using EventStore.Client;

namespace EventStore.Helpers;
public class AllStreamProjectionHandler : IProjectionHandler
{
    private readonly EventStoreClient _client;
    private readonly FromAll _start;
    private readonly Dictionary<string, List<Func<object, EventMetadata, Task>>> _handlers = new();

    private StreamSubscription? _subscription;
    private CancellationTokenSource? _stoppingTokenSource;

    public IServiceProvider Services { get; }

    public AllStreamProjectionHandler(IServiceProvider serviceProvider, FromAll start, EventStoreClient client)
    {
        Services = serviceProvider;
        _client = client;
        _start = start;
    }

    public Task Stop()
    {
        _stoppingTokenSource?.Cancel();
        return Task.CompletedTask;
    }
    public async Task Start()
    {
        _stoppingTokenSource = new CancellationTokenSource();
        _subscription = await _client.SubscribeToAllAsync(_start, OnEvent, cancellationToken: _stoppingTokenSource.Token);
    }
    private Task OnEvent(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken token) => EventHelper.OnEvent(_handlers, subscription, resolvedEvent, token);
    public void On<T>(Func<T, EventMetadata, Task> onEvent) => EventHelper.AddHandlerFunction<T>(_handlers, onEvent);
}
