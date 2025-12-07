using EventDbLite.Abstractions;
using EventDbLite.Handlers;
using EventDbLite.Handlers.Abstractions;
using EventDbLite.Reactions.Abstractions;
using System.Reflection;

namespace EventDbLite.Reactions;
public class ReactionClassContainer<T> : IReactionClassContainer<T>
    where T : class
{
    private readonly IEventStoreLite _store;
    private readonly IAsyncHandlerProvider _handlerProvider;
    private readonly IEventSerializer _eventSerializer;
    private readonly ILiveProjectionRepository _projectionRepository;

    private readonly CancellationTokenSource _cts = new();

    public T Instance { get; }
    public ReactionClassContainer(T instance, IAsyncHandlerProvider handlerProvider, IEventStoreLite store, IEventSerializer eventSerializer, ILiveProjectionRepository projectionRepository)
    {
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _handlerProvider = handlerProvider ?? throw new ArgumentNullException(nameof(handlerProvider));
        _eventSerializer = eventSerializer;
        _projectionRepository = projectionRepository;

        _ = ProcessClass(Instance);
    }
    private IEnumerable<Type> GetProjectionDependencies(T instance)
    {
        MethodInfo? methods = typeof(T).GetMethod("GetProjectionDependencies", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (methods == null)
        {
            return Enumerable.Empty<Type>();
        }

        if (methods.ReturnType != typeof(IEnumerable<Type>))
        {
            return Enumerable.Empty<Type>();
        }

        IEnumerable<Type>? result = methods.Invoke(instance, Array.Empty<object>()) as IEnumerable<Type>;

        return result ?? Enumerable.Empty<Type>();
    }

    private Task WaitForDependencies(IEnumerable<Type> dependencies, StreamPosition position)
    {
        if (dependencies == null || !dependencies.Any())
        {
            return Task.CompletedTask;
        }

        List<Task> waitTasks = new();
        foreach (Type dependency in dependencies)
        {
            ILiveProjectionManager? manager = _projectionRepository.GetManager(dependency);

            if (manager == null)
            {
                continue;
            }

            waitTasks.Add(manager.WaitForVersion(position.Version, _cts.Token));

        }
        return Task.WhenAll(waitTasks);
    }

    private async Task ProcessClass(T instance)
    {
        if (instance == null)
        {
            return;
        }

        IEnumerable<AsyncHandler> handlers = _handlerProvider.GetHandlerMethods(typeof(T));

        Dictionary<string, AsyncHandler> handlerMap = handlers.ToDictionary(h => _eventSerializer.GetIdentifier(h.TargetType));

        IStreamSubscription subscription = _store.SubscribeToAllStreams(StreamPosition.End);

        IEnumerable<Type> dependencies = GetProjectionDependencies(instance);

        try
        {
            await foreach (SubscriptionEvent streamEvent in subscription.StreamEvents(_cts.Token))
            {
                await WaitForDependencies(dependencies, streamEvent.Event.GlobalOrdinal);

                EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Event.Data.Metadata);

                if (!handlerMap.TryGetValue(metadata.Identifier, out var handler))
                {
                    continue;
                }

                object? eventObject = _eventSerializer.DeserializeEvent(streamEvent.Event.Data.Payload, handler.TargetType);

                if (eventObject is null)
                {
                    continue;
                }

                await handler.Action(instance, eventObject);
            }
        }
        finally
        {
            subscription.Dispose();
        }

    }

    public void Dispose()
    {
        if (Instance is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _cts.Cancel();
    }
}
