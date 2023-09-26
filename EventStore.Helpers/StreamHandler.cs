using EventStore.Client;

namespace EventStore.Helpers;
internal class StreamHandler : HandlerBase
{
    private readonly EventStoreClient _client;
    private readonly string _streamName;
    private readonly FromStream _start;

    private StreamSubscription? _subscription;

    public StreamHandler(EventStoreClient client, string streamName, FromStream start)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _streamName = streamName ?? throw new ArgumentNullException(nameof(streamName));
        _start = start;
    }

    protected override async Task OnStart(CancellationToken stoppingToken)
    {
        _subscription = await _client.SubscribeToStreamAsync(_streamName, _start, OnEvent, cancellationToken: stoppingToken);
    }

}
