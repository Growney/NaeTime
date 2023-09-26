using EventStore.Client;

namespace EventStore.Helpers;
internal class HandlerCollection : IHandlerCollection
{
    private readonly Dictionary<string, List<(Type eventType, Func<object, EventMetadata, Task> handler)>> _streamHandlers = new();
    private readonly List<(Type eventType, Func<object, EventMetadata, Task> handler)> _allStreamHandlers = new();

    private readonly EventStoreClient _client;

    public HandlerCollection(EventStoreClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public void Add(Type eventType, Func<object, EventMetadata, Task> handler, string? streamName = null)
    {
        if (streamName != null)
        {
            if (!_streamHandlers.TryGetValue(streamName, out var handlers))
            {
                handlers = new List<(Type eventType, Func<object, EventMetadata, Task> handler)>();
                _streamHandlers.Add(streamName, handlers);
            }
            handlers.Add((eventType, handler));
        }
        else
        {
            _allStreamHandlers.Add((eventType, handler));
        }
    }

    public IHandlerProvider BuildProvider()
    {
        var handlers = new List<IHandler>();

        foreach (var streamHandlerKvp in _streamHandlers)
        {
            var streamHandler = new StreamHandler(_client, streamHandlerKvp.Key, FromStream.Start);
            foreach (var handlerInfo in streamHandlerKvp.Value)
            {
                streamHandler.On(handlerInfo.eventType, handlerInfo.handler);
            }
            handlers.Add(streamHandler);
        }

        var allStreamHandler = new AllStreamHandler(_client, FromAll.Start);

        foreach (var handlerInfo in _allStreamHandlers)
        {
            allStreamHandler.On(handlerInfo.eventType, handlerInfo.handler);
        }

        handlers.Add(allStreamHandler);

        return new HandlerProvider(handlers);
    }
}
