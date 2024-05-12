using Microsoft.Extensions.DependencyInjection;

namespace NaeTime.PubSub.Abstractions;
public static class IRemoteProcedureCallRegistrarExtensions
{
    public static void RegisterHandler<TResponse>(this IRemoteProcedureCallRegistrar manager, string name, Func<Task<TResponse>> handler)
    {
        RPCSignature signature = new(name, typeof(TResponse));

        manager.RegisterHandler(signature, async parameters =>
        {
            object? result = await handler();

            return result == null ? null : (TResponse)result;
        });
    }
    public static void RegisterHandler<T1, TResponse>(this IRemoteProcedureCallRegistrar manager, string name, Func<T1, Task<TResponse>> handler)
    {
        RPCSignature signature = new(name, typeof(TResponse), typeof(T1));

        manager.RegisterHandler(signature, async parameters =>
        {
            T1 parameter = (T1)parameters[0]!;

            object? result = await handler(parameter);

            return result == null ? null : (TResponse)result;
        });
    }
    public static void RegisterHandler<T1, T2, TResponse>(this IRemoteProcedureCallRegistrar manager, string name, Func<T1, T2, Task<TResponse>> handler)
    {
        RPCSignature signature = new(name, typeof(TResponse), typeof(T1));

        manager.RegisterHandler(signature, async parameters =>
        {
            T1 arg1 = (T1)parameters[0]!;
            T2 arg2 = (T2)parameters[1]!;

            object? result = await handler(arg1, arg2);

            return result == null ? null : (TResponse)result;
        });
    }
    public static void RegisterHandler<T1, T2, T3, TResponse>(this IRemoteProcedureCallRegistrar manager, string name, Func<T1, T2, T3, Task<TResponse>> handler)
    {
        RPCSignature signature = new(name, typeof(TResponse), typeof(T1));

        manager.RegisterHandler(signature, async parameters =>
        {
            T1 arg1 = (T1)parameters[0]!;
            T2 arg2 = (T2)parameters[1]!;
            T3 arg3 = (T3)parameters[2]!;

            object? result = await handler(arg1, arg2, arg3);

            return result == null ? null : (TResponse)result;
        });
    }
    public static void RegisterHub(this IRemoteProcedureCallRegistrar manager, object hub)
    {
        RPCHubHandlerFactory handlerFactory = new();

        IEnumerable<(RPCSignature, Func<object, object?[], Task<object?>>)> handlers = handlerFactory.GetTypeHandlers(hub.GetType());

        foreach ((RPCSignature signature, Func<object, object?[], Task<object?>> handler) in handlers)
        {
            manager.RegisterHandler(signature, parameters => handler(hub, parameters));
        }
    }

    public static void RegisterServiceHub<THub>(this IRemoteProcedureCallRegistrar manager, IServiceProvider services) => RegisterServiceHub(manager, typeof(THub), services);
    public static void RegisterServiceHub(this IRemoteProcedureCallRegistrar manager, Type hubType, IServiceProvider services)
    {
        RPCHubHandlerFactory handlerFactory = new();

        IEnumerable<(RPCSignature, Func<object, object?[], Task<object?>>)> handlers = handlerFactory.GetTypeHandlers(hubType);

        foreach ((RPCSignature signature, Func<object, object?[], Task<object?>> handler) in handlers)
        {
            manager.RegisterHandler(signature, parameters =>
            {
                object? hubInstance = services.GetService(hubType) ?? throw new InvalidOperationException($"Could not create an instance of {hubType.Name}");

                return handler(hubInstance, parameters);
            });
        }
    }
    public static void RegisterTransientHub(this IRemoteProcedureCallRegistrar manager, Type hubType, IServiceProvider services)
    {
        RPCHubHandlerFactory handlerFactory = new();

        IEnumerable<(RPCSignature, Func<object, object?[], Task<object?>>)> handlers = handlerFactory.GetTypeHandlers(hubType);

        foreach ((RPCSignature signature, Func<object, object?[], Task<object?>> handler) in handlers)
        {
            manager.RegisterHandler(signature, parameters =>
            {
                object? hubInstance = ActivatorUtilities.CreateInstance(services, hubType) ?? throw new InvalidOperationException($"Could not create an instance of {hubType.Name}");

                return handler(hubInstance, parameters);
            });
        }
    }
    public static void RegisterTransientHub<THub>(this IRemoteProcedureCallRegistrar manager, IServiceProvider services) => RegisterTransientHub(manager, typeof(THub), services);
    public static void RegisterScopedHub(this IRemoteProcedureCallRegistrar manager, Type hubType, IServiceProvider services)
    {
        RPCHubHandlerFactory handlerFactory = new();

        IEnumerable<(RPCSignature, Func<object, object?[], Task<object?>>)> handlers = handlerFactory.GetTypeHandlers(hubType);

        foreach ((RPCSignature signature, Func<object, object?[], Task<object?>> handler) in handlers)
        {
            manager.RegisterHandler(signature, async parameters =>
            {
                IServiceScope scope = services.CreateScope();

                object hubInstance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, hubType) ?? throw new InvalidOperationException($"Could not create an instance of {hubType.Name}");
                object? result;
                try
                {
                    result = await handler(hubInstance, parameters);
                }
                finally
                {
                    scope.Dispose();
                }

                return result;
            });
        }
    }
    public static void RegisterScopedHub<THub>(this IRemoteProcedureCallRegistrar manager, IServiceProvider serviceProvider) => RegisterScopedHub(manager, typeof(THub), serviceProvider);
}
