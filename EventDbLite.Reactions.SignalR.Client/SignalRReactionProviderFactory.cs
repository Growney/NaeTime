using EventDbLite.Abstractions;
using EventDbLite.Reactions.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace EventDbLite.Reactions.SignalR.Client;
public class SignalRReactionProviderFactory : IReactionProviderFactory
{

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
        => new SignalRReactionProvider<TEvent>(streamName,_baseAddress, initialPosition, _eventSerializer, _httpClientFactory);
}
