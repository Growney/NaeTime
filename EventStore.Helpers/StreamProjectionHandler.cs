using EventStore.Client;

namespace EventStore.Helpers;
public class StreamProjectionHandler : IProjectionHandler
{
    private readonly EventStoreClient _client;
    private readonly string _streamName;
    private readonly FromStream _start;
    private readonly Dictionary<string, List<Func<object, EventMetadata, Task>>> _handlers = new();

    private StreamSubscription? _subscription;
    private CancellationTokenSource? _stoppingTokenSource;


    public IServiceProvider Services { get; }
    public StreamProjectionHandler(IServiceProvider serviceProvider, string streamName, FromStream start, EventStoreClient client)
    {
        Services = serviceProvider;
        _streamName = streamName;
        _start = start;
        _client = client;
    }
    public Task Stop()
    {
        _stoppingTokenSource?.Cancel();
        return Task.CompletedTask;
    }
    public async Task Start()
    {
        _stoppingTokenSource = new CancellationTokenSource();
        _subscription = await _client.SubscribeToStreamAsync(_streamName, _start, OnEvent);
    }
    private Task OnEvent(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken token) => EventHelper.OnEvent(_handlers, subscription, resolvedEvent, token);
    public void On<T>(Func<T, EventMetadata, Task> onEvent) => EventHelper.AddHandlerFunction<T>(_handlers, onEvent);
}
