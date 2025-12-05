using EventDbLite.Abstractions;
using EventDbLite.Reactions.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace EventDbLite.Reactions.SignalR.Client;
public class SignalRReactionProviderFactory : IHostedService, IReactionProviderFactory
{
    private ConcurrentDictionary<Guid, IEventConsumer> _consumers = new();

    private HubConnection? _hubConnection;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEventSerializer _eventSerializer;
    private readonly string _baseAddress;

    public SignalRReactionProviderFactory(IHttpClientFactory httpClientFactory, IEventSerializer eventSerializer, string baseAddress)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _baseAddress = baseAddress;
    }

    public IAsyncEnumerable<ReactionEvent<TEvent>> CreateProvider<TEvent>(StreamPosition initialPosition, string? streamName = null)
    {
        Guid providerId = Guid.NewGuid();

        void RemoveConsumer()
        {
            _consumers.TryRemove(providerId, out _);
        }

        ReactionProvider<TEvent> provider = new(streamName, initialPosition, _eventSerializer, _httpClientFactory, RemoveConsumer);

        _consumers.TryAdd(providerId, provider);

        return provider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_baseAddress}/eventDbLiteHub")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On("ReceiveEvent", (string streamName, StreamEvent streamEvent) =>
        {
            foreach (IEventConsumer consumer in _consumers.Values)
            {
                consumer.AddEvent(streamEvent);
            }
        });

        await _hubConnection.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_hubConnection == null)
        {
            return Task.CompletedTask;
        }

        return _hubConnection.StopAsync();
    }
}
