using EventDbLite.Abstractions;
using EventDbLite.Handlers.Abstractions;
using System.Collections.Concurrent;
using System.Reflection;

namespace EventDbLite.Handlers;

public class AsyncHandlerProvider(IEventSerializer eventSerializer) : IAsyncHandlerProvider
{
    private readonly ConcurrentDictionary<Type, Dictionary<string, AsyncHandler>> _handlerMethods = new();

    private readonly IEventSerializer _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));

    private Dictionary<string, AsyncHandler> RegisterAggregateRoot(Type aggregateRootType)
    {
        Dictionary<string, AsyncHandler> handlerMethods = [];
        foreach (MethodInfo method in aggregateRootType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
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

            handlerMethods.Add(identifier, GetHandler(eventType, method));
        }

        return handlerMethods;
    }

    private AsyncHandler GetHandler(Type targetType, MethodInfo method)
        => method.ReturnType == typeof(Task)
            ? GetAsyncHandler(targetType, method)
            : GetVoidHandler(targetType, method);

    private AsyncHandler GetAsyncHandler(Type targetType, MethodInfo method)
        => new((instance, eventObj) =>
        {
            return method.Invoke(instance, [eventObj]) is not Task result
                ? throw new InvalidOperationException($"Method {method.Name} in {instance.GetType().FullName} must return a Task or void.")
                : result;
        }, targetType);

    private AsyncHandler GetVoidHandler(Type targetType, MethodInfo method)
        => new((instance, eventObj) =>
        {
            method.Invoke(instance, [eventObj]);
            return Task.CompletedTask;
        }, targetType);

    public AsyncHandler? GetHandlerMethod(Type handlerType, string identifier)
    {
        Dictionary<string, AsyncHandler> handlerMethods = _handlerMethods.GetOrAdd(handlerType, RegisterAggregateRoot);

        handlerMethods.TryGetValue(identifier, out AsyncHandler? method);

        return method;
    }
    public IEnumerable<AsyncHandler> GetHandlerMethods(Type handlerType)
    {
        Dictionary<string, AsyncHandler> handlerMethods = _handlerMethods.GetOrAdd(handlerType, RegisterAggregateRoot);
        return handlerMethods.Values;
    }
}
