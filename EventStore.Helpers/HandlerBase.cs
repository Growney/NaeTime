using EventStore.Client;

namespace EventStore.Helpers;
internal abstract class HandlerBase : IHandler
{
    private readonly Dictionary<string, List<Func<object, EventMetadata, Task>>> _handlers = new();
    private readonly Dictionary<string, Func<EventMetadata, ReadOnlyMemory<byte>, Task>> _eventConstructors = new();

    private bool _isRunning;
    private CancellationTokenSource? _stoppingTokenSource;

    public void On(Type eventType, Func<object, EventMetadata, Task> handler)
    {
        var eventKey = EventHelper.GetEventTypeName(eventType);

        if (!_eventConstructors.ContainsKey(eventKey))
        {
            _eventConstructors.Add(eventKey, ConstructEvent(eventType));
        }

        if (!_handlers.TryGetValue(eventKey, out var handlerList))
        {
            handlerList = new List<Func<object, EventMetadata, Task>>();
            _handlers.Add(eventKey, handlerList);
        }

        handlerList.Add(handler);

    }
    protected Task OnEvent(StreamSubscription subscription, ResolvedEvent resolvedEvent, CancellationToken token) => HandleAsync(resolvedEvent.Event);
    protected async Task HandleAsync(EventRecord data)
    {
        var eventMetadata = EventHelper.CreateEventMetadata(data);

        if (eventMetadata == null)
        {
            //TODO log no metadata
            return;
        }

        if (!_eventConstructors.TryGetValue(eventMetadata.TypeName, out var eventConstructor))
        {
            //TODO log no constructor
            return;
        }

        await eventConstructor(eventMetadata, data.Data);
    }
    private Func<EventMetadata, ReadOnlyMemory<byte>, Task> ConstructEvent(Type eventType)
    => async (eventMetadata, eventData) =>
        {
            var eventTypeName = EventHelper.GetEventTypeName(eventType);

            if (!_handlers.ContainsKey(eventTypeName))
            {
                //TODO log no handlers
                return;
            }

            var eventObj = EventHelper.CreateEvent(eventType, eventData);

            if (eventObj == null)
            {
                //TODO log event not found
                return;
            }

            var handlers = _handlers[eventTypeName];

            foreach (var handler in handlers)
            {
                try
                {
                    await handler(eventObj, eventMetadata);
                }
                catch (Exception)
                {
                    //TODO log exception
                }
            }
        };

    public async Task Start()
    {
        if (_isRunning)
        {
            throw new InvalidOperationException("Already running");
        }
        _stoppingTokenSource?.Cancel();
        _stoppingTokenSource = new CancellationTokenSource();
        await OnStart(_stoppingTokenSource.Token);
        _isRunning = true;
    }
    protected abstract Task OnStart(CancellationToken stoppingToken);
    public Task Stop()
    {
        if (!_isRunning)
        {
            throw new InvalidOperationException("Not running");
        }
        _stoppingTokenSource?.Cancel();
        _isRunning = false;
        return Task.CompletedTask;
    }


}
