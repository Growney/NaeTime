using NaeTime.PubSub.Abstractions;
using System.Reflection;

namespace NaeTime.PubSub;
public class RPCHubHandlerFactory
{
    public IEnumerable<(RPCSignature signature, Func<object, object?[], Task<object?>>)> GetTypeHandlers(Type type)
    {
        List<(RPCSignature, Func<object, object?[], Task<object?>>)> values = new();
        MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);

        foreach (MethodInfo method in methods)
        {
            if (method.ReturnType == typeof(void))
            {
                continue;
            }

            if (method.ReturnType == typeof(Task))
            {
                continue;
            }

            //We only want to pick up methods that are directly declared on the type
            if (method.DeclaringType != type)
            {
                continue;
            }

            Type[] parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

            bool isAsync = method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
            Func<object, object?[], Task<object?>> handler = isAsync ? CreateGenericTaskResponse(method) : CreateResponse(method);
            Type returnType = isAsync ? method.ReturnType.GetGenericArguments().First() : method.ReturnType;

            RPCSignature signature = new(method.Name, returnType, parameterTypes);

            values.Add((signature, handler));
        }

        return values;
    }
    private static Func<object, object?[], Task<object?>> CreateGenericTaskResponse(MethodInfo info)
        => async (object obj, object?[] parameters) =>
        {
            dynamic? methodResult = info.Invoke(obj, parameters);

            if (methodResult is null)
            {
                return null;
            }

            if (methodResult is Task task)
            {
                await task.ConfigureAwait(false);
            }

            return methodResult.Result;
        };
    private static Func<object, object?[], Task<object?>> CreateResponse(MethodInfo info)
        => (object obj, object?[] parameters) =>
        {
            object? methodResult = info.Invoke(obj, parameters);

            if (methodResult is null)
            {
                return Task.FromResult<object?>(null);
            }

            return Task.FromResult<object?>(methodResult);
        };
}
