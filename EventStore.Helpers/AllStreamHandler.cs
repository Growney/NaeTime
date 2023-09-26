using EventStore.Client;

namespace EventStore.Helpers;
internal class AllStreamHandler : HandlerBase
{
    private readonly EventStoreClient _client;
    private readonly FromAll _start;

    private StreamSubscription? _subscription;

    public AllStreamHandler(EventStoreClient client, FromAll start)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _start = start;
    }

    protected override async Task OnStart(CancellationToken stoppingToken)
    {
        _subscription = await _client.SubscribeToAllAsync(_start, OnEvent, cancellationToken: stoppingToken);
    }
}
