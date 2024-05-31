using NaeTime.PubSub.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.PubSub;
public class EventManager : IEventClient, IEventRegistrar
{
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<Type>> _keyedHandlers = new();
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, ConcurrentBag<Func<object, Task>>>> _handlers = new();

    private class EventManagerScope : IEventRegistrarScope
    {
        private readonly EventManager _eventManager;
        private readonly Guid _scopeId;

        public EventManagerScope(Guid scopeId, EventManager eventManager)
        {
            _scopeId = scopeId;
            _eventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
        }

        public IEventRegistrarScope CreateScope() => _eventManager.CreateScope();
        public void Dispose() => _eventManager.RemoveHandlerKey(_scopeId);
        public void RegisterHandler(Type type, Func<object, Task> handler) => _eventManager.RegisterHandler(_scopeId, type, handler);
    }

    public IEventRegistrarScope CreateScope() => new EventManagerScope(Guid.NewGuid(), this);
    public Task PublishAsync(object obj)
    {
        Type type = obj.GetType();

        if (!_handlers.TryGetValue(type, out ConcurrentDictionary<Guid, ConcurrentBag<Func<object, Task>>>? handlerTypeKeyHandlers))
        {
            return Task.CompletedTask;
        }

        List<Task> tasks = new();

        foreach (ConcurrentBag<Func<object, Task>> handlers in handlerTypeKeyHandlers.Values)
        {
            foreach (Func<object, Task> handler in handlers)
            {
                tasks.Add(handler(obj));
            }
        }

        return Task.WhenAll(tasks);
    }
    public void RegisterHandler(Type type, Func<object, Task> handler) => RegisterHandler(Guid.Empty, type, handler);
    private void RegisterHandler(Guid key, Type type, Func<object, Task> handler)
    {
        ConcurrentBag<Type> types = _keyedHandlers.GetOrAdd(key, _ => new ConcurrentBag<Type>());

        if (!types.Contains(type))
        {
            types.Add(type);
        }

        ConcurrentDictionary<Guid, ConcurrentBag<Func<object, Task>>> handlerTypeKeyHandlers = _handlers.GetOrAdd(type, _ => new ConcurrentDictionary<Guid, ConcurrentBag<Func<object, Task>>>());

        ConcurrentBag<Func<object, Task>> handlers = handlerTypeKeyHandlers.GetOrAdd(key, _ => new ConcurrentBag<Func<object, Task>>());

        handlers.Add(handler);
    }
    private void RemoveHandlerKey(Guid key)
    {
        if (!_keyedHandlers.TryGetValue(key, out ConcurrentBag<Type>? types))
        {
            return;
        }

        foreach (Type type in types)
        {
            if (_handlers.TryGetValue(type, out ConcurrentDictionary<Guid, ConcurrentBag<Func<object, Task>>>? handlerTypeKeyHandlers))
            {
                handlerTypeKeyHandlers.TryRemove(key, out _);
            }
        }

        _keyedHandlers.TryRemove(key, out _);
    }
}
