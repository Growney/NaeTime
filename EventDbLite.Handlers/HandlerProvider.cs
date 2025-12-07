using EventDbLite.Abstractions;
using EventDbLite.Handlers.Abstractions;
using System.Collections.Concurrent;
using System.Reflection;

namespace EventDbLite.Handlers;

public class HandlerProvider(IEventSerializer eventSerializer) : IHandlerProvider
{
    private readonly ConcurrentDictionary<Type, Dictionary<string, Handler>> _handlerMethods = new();

    private readonly IEventSerializer _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));

    private Dictionary<string, Handler> RegisterHandler(Type aggregateRootType)
    {
        Dictionary<string, Handler> handlerMethods = [];
        foreach (MethodInfo method in aggregateRootType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
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

            Handler handler = new((instance, handleObj) =>
            {
                MethodInfo capturedMethod = method;
                capturedMethod.Invoke(instance, new[] { handleObj });
            }, eventType);

            handlerMethods.Add(identifier, handler);
        }

        return handlerMethods;
    }

    public Handler? GetHandlerMethod(Type handlerType, string identifier)
    {
        Dictionary<string, Handler> handlerMethods = _handlerMethods.GetOrAdd(handlerType, RegisterHandler);

        if (!handlerMethods.TryGetValue(identifier, out Handler? method))
        {
            return null;
        }

        return method;
    }

    public IEnumerable<Handler> GetAllHandlerMethods(Type handlerType)
    {
        Dictionary<string, Handler> handlerMethods = _handlerMethods.GetOrAdd(handlerType, RegisterHandler);
        return handlerMethods.Values;
    }
}
