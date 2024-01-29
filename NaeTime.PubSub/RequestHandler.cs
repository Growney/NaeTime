using System.Reflection;

namespace NaeTime.PubSub;
public class RequestHandler
{
    private readonly Type _requestType;
    private readonly Type _responseType;
    private Func<object> _handler;

    private readonly MethodInfo _onMethodInfo;

    public RequestHandler(Type handlerType, Type requestType, Type responseType, Func<object> handler)
    {
        var method = FindMethodInfo(handlerType, requestType, responseType);

        if (method == null)
        {
            throw new ArgumentException($"Handler method not found");
        }

        _onMethodInfo = method;

        _requestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
        _responseType = responseType ?? throw new ArgumentNullException(nameof(responseType));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
    public async Task<TResponse?> Handle<TResponse>(object request)
    {
        if (request.GetType() != _requestType)
        {
            throw new ArgumentException("Request type does not match handler");
        }

        var handler = _handler();

        var result = _onMethodInfo.Invoke(handler, [request]);

        if (result is Task<TResponse> task)
        {
            var response = await task;

            return response;
        }

        return default;
    }
    private MethodInfo? FindMethodInfo(Type handlerType, Type requestType, Type responseType)
    {
        var methods = handlerType.GetMethods();

        foreach (var method in methods)
        {
            if (method.Name != "On")
            {
                continue;
            }
            var parameters = method.GetParameters();

            if (parameters.Length != 1)
            {
                continue;
            }

            var parameter = parameters[0];

            if (parameter.ParameterType != requestType)
            {
                continue;
            }

            var returnType = method.ReturnType;

            if (returnType.IsGenericType)
            {
                var rootType = returnType.GetGenericTypeDefinition();
                if (rootType != typeof(Task<>))
                {
                    continue;
                }

                var genericArgument = returnType.GetGenericArguments().FirstOrDefault();

                if (genericArgument == null)
                {
                    continue;
                }

                if (genericArgument != responseType)
                {
                    continue;
                }

            }
            else
            {
                if (returnType != responseType)
                {
                    continue;
                }

            }

            return method;

        }

        return null;
    }
}
