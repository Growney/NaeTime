using System.Reflection;

namespace EventStore.Helpers;
public static class IHandlerCollectionExtensions
{
    public static void Add<TEvent>(this IHandlerCollection collection, Func<TEvent, EventMetadata, Task> handler, string? streamName = null) => collection.Add(typeof(TEvent), (e, m) => handler((TEvent)e, m), streamName);
    public static void Add<TEvent>(this IHandlerCollection collection, Func<TEvent, Task> handler, string? streamName = null) => collection.Add(typeof(TEvent), (e, _) => handler((TEvent)e), streamName);

    private static IEnumerable<MethodInfo> GetValidHandlerMethods(Type handlerClassType)
    {
        var methods = handlerClassType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (var method in methods)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 1)
            {
                if (parameters[0].ParameterType != typeof(object))
                {
                    continue;
                }
                if (method.ReturnType != typeof(Task))
                {
                    continue;
                }
                yield return method;
            }
            else if (parameters.Length == 2)
            {
                if (parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(EventMetadata))
                {
                    continue;
                }
                else if (parameters[1].ParameterType == typeof(object) && parameters[0].ParameterType == typeof(EventMetadata))
                {
                    continue;
                }

                if (method.ReturnType != typeof(Task))
                {
                    continue;
                }
                yield return method;
            }
        }
    }
    private static Type GetMethodHandlerEventType(MethodInfo info)
    {
        var parameters = info.GetParameters();
        if (parameters.Length == 0)
        {
            throw new InvalidOperationException("Invalid handler method");
        }
        if (parameters.Length == 1)
        {
            var firstParameterType = parameters[0].ParameterType;

            if (firstParameterType != typeof(object) || firstParameterType == typeof(EventMetadata))
            {
                throw new InvalidOperationException("Invalid handler method");
            }
            return firstParameterType;
        }
        else if (parameters.Length == 2)
        {
            var firstParameterType = parameters[0].ParameterType;
            var secondParameterType = parameters[1].ParameterType;

            if (firstParameterType == typeof(object) && firstParameterType != typeof(EventMetadata))
            {
                return firstParameterType;
            }
            else if (secondParameterType == typeof(object) && secondParameterType != typeof(EventMetadata))
            {
                return secondParameterType;
            }
        }

        throw new InvalidOperationException("Invalid handler method");
    }

    private static object[] GetOrderedParameters(MethodInfo info, object eventObj, EventMetadata metadata)
    {
        var parameters = info.GetParameters();
        if (parameters.Length == 0)
        {
            throw new InvalidOperationException("Invalid handler method");
        }
        if (parameters.Length == 1)
        {
            var firstParameterType = parameters[0].ParameterType;

            if (firstParameterType != typeof(object) || firstParameterType == typeof(EventMetadata))
            {
                throw new InvalidOperationException("Invalid handler method");
            }
            return new object[] { eventObj };
        }
        else if (parameters.Length == 2)
        {
            var firstParameterType = parameters[0].ParameterType;
            var secondParameterType = parameters[1].ParameterType;

            if (firstParameterType == typeof(object) && secondParameterType == typeof(EventMetadata))
            {
                return new object[] { eventObj, metadata };
            }
            else if (secondParameterType == typeof(object) && firstParameterType == typeof(EventMetadata))
            {
                return new object[] { metadata, eventObj };
            }
        }

        throw new InvalidOperationException("Invalid handler method");
    }

    public static void Add(this IHandlerCollection collection, Type handlerType, Func<object> instanceFactory, string? streamName = null)
    {
        var methods = GetValidHandlerMethods(handlerType);
        foreach (var method in methods)
        {
            var methodHandlerType = GetMethodHandlerEventType(method);

            collection.Add(handlerType, (eventObj, metadata) =>
            {
                var orderedParameters = GetOrderedParameters(method, eventObj, metadata);

                var instance = instanceFactory();

                return method.Invoke(instance, orderedParameters) as Task ?? Task.CompletedTask;

            }, streamName);
        }
    }

}
