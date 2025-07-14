using EventDbLite.Abstractions;
using System.Collections.Concurrent;
using System.Reflection;

namespace EventDbLite.Handlers;

public class CommandHandlerProvider : ICommandHandlerProvider
{
    private readonly ConcurrentDictionary<Type, Dictionary<string, (Type targetType, Func<object, object, Task<bool>> handler)>> _handlerMethods = new();

    private readonly IEventSerializer _eventSerializer;

    public CommandHandlerProvider(IEventSerializer eventSerializer)
    {
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    }

    private Dictionary<string, (Type targetType, Func<object, object, Task<bool>> handler)> RegisterAggregateRoot(Type commandControllerType)
    {
        Dictionary<string, (Type targetType, Func<object, object, Task<bool>> handler)> handlerMethods = new();
        foreach (MethodInfo method in commandControllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (method.Name != "On")
            {
                continue;
            }

            if (method.ReturnType != typeof(bool) && method.ReturnType != typeof(Task<bool>))
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
                throw new InvalidOperationException($"Duplicate handler method found: {identifier} in {commandControllerType.FullName}");
            }

            handlerMethods.Add(identifier, (eventType, GetHandler(method)));
        }

        return handlerMethods;
    }

    private Func<object, object, Task<bool>> GetHandler(MethodInfo method)
        => method.ReturnType == typeof(Task)
            ? GetAsyncHandler(method)
            : GetVoidHandler(method);

    private Func<object, object, Task<bool>> GetAsyncHandler(MethodInfo method)
        => (instance, eventObj) => method.Invoke(instance, [eventObj]) is not Task<bool> result
                ? throw new InvalidOperationException($"Method {method.Name} in {instance.GetType().FullName} must return a Task<bool>.")
                : result;

    private Func<object, object, Task<bool>> GetVoidHandler(MethodInfo method)
        => (instance, eventObj) =>
        {
            object? nullResult = method.Invoke(instance, [eventObj]);

            if (nullResult is null or not bool)
            {
                throw new InvalidOperationException($"Method {method.Name} in {instance.GetType().FullName} must return a bool.");
            }

            return Task.FromResult((bool)nullResult);
        };

    public CommandHandler? GetHandlerMethod(object handler, string identifier)
    {
        Type handlerType = handler.GetType();

        Dictionary<string, (Type targetType, Func<object, object, Task<bool>> handler)> handlerMethods = _handlerMethods.GetOrAdd(handlerType, RegisterAggregateRoot);

        handlerMethods.TryGetValue(identifier, out (Type targetType, Func<object, object, Task<bool>> handler) method);

        return new CommandHandler(
            action: payload => method.handler(handler, payload),
            targetType: method.targetType
        );
    }
}
