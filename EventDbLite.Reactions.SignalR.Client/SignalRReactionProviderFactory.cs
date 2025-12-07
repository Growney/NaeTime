using EventDbLite.Abstractions;
using EventDbLite.Reactions.Abstractions;

namespace EventDbLite.Reactions.SignalR.Client;
public class SignalRReactionProviderFactory : IReactionProviderFactory
{

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEventSerializer _eventSerializer;
    private readonly IEventClient _eventClient;

    public SignalRReactionProviderFactory(IHttpClientFactory httpClientFactory, IEventSerializer eventSerializer, IEventClient eventClient)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
    }

    public IAsyncEnumerable<ReactionEvent<TEvent>> CreateProvider<TEvent>(StreamPosition initialPosition, string? streamName = null)
        => new SignalRReactionProvider<TEvent>(streamName, initialPosition, _eventSerializer, _httpClientFactory, _eventClient);
}
