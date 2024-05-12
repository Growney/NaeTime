using Microsoft.Extensions.DependencyInjection;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.PubSub.Extensions;
public static class IRemoteProcedureCallRegistrarExtensions
{
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
