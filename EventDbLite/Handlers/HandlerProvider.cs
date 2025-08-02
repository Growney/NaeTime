using EventDbLite.Abstractions;
using System.Collections.Concurrent;
using System.Reflection;

namespace EventDbLite.Handlers;

internal class HandlerProvider : IHandlerProvider
{
    private readonly ConcurrentDictionary<Type, Dictionary<string, (Type targetType, Action<object, object> handler)>> _handlerMethods = new();

    private readonly IEventSerializer _eventSerializer;

    public HandlerProvider(IEventSerializer eventSerializer)
    {
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    }

    private Dictionary<string, (Type targetType, Action<object, object> handler)> RegisterHandler(Type aggregateRootType)
    {
        Dictionary<string, (Type targetType, Action<object, object> handler)> handlerMethods = new();
        foreach (MethodInfo method in aggregateRootType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (method.Name != "When")
            {
                continue;
            }

            if (method.ReturnType != typeof(void))
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

            handlerMethods.Add(identifier, (eventType, (instance, eventObj) =>
            {
                MethodInfo info = method;
                info.Invoke(instance, [eventObj]);
            }
            ));
        }

        return handlerMethods;
    }

    public Handler? GetHandlerMethod(object handler, string identifier)
    {
        Type handlerType = handler.GetType();

        Dictionary<string, (Type targetType, Action<object, object> handler)> handlerMethods = _handlerMethods.GetOrAdd(handlerType, RegisterHandler);

        handlerMethods.TryGetValue(identifier, out (Type targetType, Action<object, object> handler) method);

        return new Handler(
            action: payload => method.handler(handler, payload),
            targetType: method.targetType
        );
    }
}
