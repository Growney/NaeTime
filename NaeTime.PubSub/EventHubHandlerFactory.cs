using System.Data;
using System.Reflection;

namespace NaeTime.PubSub;
public class EventHubHandlerFactory
{
    public IEnumerable<(Type, Func<object, object, Task>)> GetTypeHandlers(Type type)
    {
        List<(Type, Func<object, object, Task>)> handlers = new();

        MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);

        foreach (MethodInfo method in methods)
        {
            if (!IsValidReturnType(method.ReturnType))
            {
                continue;
            }

            if (method.DeclaringType != type)
            {
                continue;
            }

            if (!method.Name.Equals("When", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Type[] parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();

            if (parameterTypes.Length != 1)
            {
                continue;
            }

            Type commandType = parameterTypes[0];

            async Task Handler(object obj, object parameter)
            {
                object? methodResult = method.Invoke(obj, [parameter]);

                if (methodResult is Task task)
                {
                    await task;
                }
            }

            handlers.Add((commandType, Handler));
        }

        return handlers;
    }
    private bool IsValidReturnType(Type type) => type == typeof(void) || type == typeof(Task);
}
