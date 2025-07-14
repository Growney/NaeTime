using EventDbLite.Abstractions;
using System.Collections.Concurrent;
using System.Reflection;

namespace EventDbLite.Handlers;

internal class AsyncHandlerProvider : IAsyncHandlerProvider
{
    private readonly ConcurrentDictionary<Type, Dictionary<string, (Type targetType, Func<object, object, Task> handler)>> _handlerMethods = new();

    private readonly IEventSerializer _eventSerializer;

    public AsyncHandlerProvider(IEventSerializer eventSerializer)
    {
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    }

    private Dictionary<string, (Type targetType, Func<object, object, Task> handler)> RegisterAggregateRoot(Type aggregateRootType)
    {
        Dictionary<string, (Type targetType, Func<object, object, Task> handler)> handlerMethods = new();
        foreach (MethodInfo method in aggregateRootType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (method.Name != "When")
            {
                continue;
            }

            if (method.ReturnType != typeof(void) && method.ReturnType != typeof(Task))
            {
                continue;
            }

            string? identifier = null;
            ParameterInfo[] methodParameters = method.GetParameters();
            if (methodParameters.Length != 1)
            {
                continue; // Skip methods that do not have exactly one parameter
            }

            ParameterInfo eventParameter = methodParameters[0];

            Type eventType = eventParameter.ParameterType;

            identifier = _eventSerializer.GetIdentifier(eventType);

            if (identifier is null)
            {
                continue; // Skip invalid methods
            }

            if (handlerMethods.ContainsKey(identifier))
            {
                throw new InvalidOperationException($"Duplicate handler method found: {identifier} in {aggregateRootType.FullName}");
            }

            handlerMethods.Add(identifier, (eventType, GetHandler(method)));
        }

        return handlerMethods;
    }

    private Func<object, object, Task> GetHandler(MethodInfo method)
        => method.ReturnType == typeof(Task)
            ? GetAsyncHandler(method)
            : GetVoidHandler(method);

    private Func<object, object, Task> GetAsyncHandler(MethodInfo method)
        => (instance, eventObj) => method.Invoke(instance, [eventObj]) is not Task result
                ? throw new InvalidOperationException($"Method {method.Name} in {instance.GetType().FullName} must return a Task or void.")
                : result;

    private Func<object, object, Task> GetVoidHandler(MethodInfo method)
        => (instance, eventObj) =>
        {
            method.Invoke(instance, [eventObj]);
            return Task.CompletedTask;
        };

    public AsyncHandler? GetHandlerMethod(object handler, string identifier)
    {
        Type handlerType = handler.GetType();

        Dictionary<string, (Type targetType, Func<object, object, Task> handler)> handlerMethods = _handlerMethods.GetOrAdd(handlerType, RegisterAggregateRoot);

        handlerMethods.TryGetValue(identifier, out (Type targetType, Func<object, object, Task> handler) method);

        return new AsyncHandler(
            action: payload => method.handler(handler, payload),
            targetType: method.targetType
        );
    }
}
