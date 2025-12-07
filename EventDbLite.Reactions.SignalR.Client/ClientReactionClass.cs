using EventDbLite.Abstractions;
using EventDbLite.Handlers;
using EventDbLite.Handlers.Abstractions;
using EventDbLite.Reactions.Abstractions;

namespace EventDbLite.Reactions.SignalR.Client;
public class ClientReactionClass<TContainer> : IReactionClassContainer<TContainer>
    where TContainer : class
{
    public TContainer Instance { get; }

    private readonly IEventClient _eventClient;
    private readonly IAsyncHandlerProvider _handlerProvider;
    private readonly IEventSerializer _eventSerializer;
    private readonly Dictionary<string, AsyncHandler> _handlerMap;

    public ClientReactionClass(TContainer instance, IAsyncHandlerProvider handlerProvider, IEventClient eventClient, IEventSerializer eventSerializer)
    {
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _handlerProvider = handlerProvider ?? throw new ArgumentNullException(nameof(handlerProvider));
        _eventSerializer = eventSerializer;

        IEnumerable<AsyncHandler> handlers = _handlerProvider.GetHandlerMethods(typeof(TContainer));

        _handlerMap = handlers.ToDictionary(h => _eventSerializer.GetIdentifier(h.TargetType));

        _eventClient.OnEventReceived += EventReceived;
    }

    private async Task EventReceived(StreamEvent streamEvent)
    {
        EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);
        if (!_handlerMap.TryGetValue(metadata.Identifier, out var handler))
        {
            return;
        }

        object? eventObject = _eventSerializer.DeserializeEvent(streamEvent.Data.Payload, handler.TargetType);

        if (eventObject is null)
        {
            return;
        }

        await handler.Action(Instance, eventObject);
    }
    public void Dispose()
    {
        _eventClient.OnEventReceived -= EventReceived;
        if (Instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
