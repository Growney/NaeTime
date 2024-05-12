using Microsoft.Extensions.DependencyInjection;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.PubSub.Extensions;
public static class IEventRegistarExtensions
{
    public static void RegisterHandler<THandler>(this IEventRegistrar registrar, Func<THandler, Task> handler) => registrar.RegisterHandler(typeof(THandler), obj => handler((THandler)obj));
    public static void RegisterHub(this IEventRegistrar registrar, object hub)
    {
        EventHubHandlerFactory handlerFactory = new();

        IEnumerable<(Type, Func<object, object, Task>)> handlers = handlerFactory.GetTypeHandlers(hub.GetType());

        foreach ((Type handlerType, Func<object, object, Task> handler) in handlers)
        {
            registrar.RegisterHandler(handlerType, obj => handler(hub, obj));
        }
    }
    public static void RegisterServiceHub<THub>(this IEventRegistrar registrar, IServiceProvider services) => RegisterServiceHub(registrar, typeof(THub), services);
    public static void RegisterServiceHub(this IEventRegistrar registrar, Type hubType, IServiceProvider services)
    {
        EventHubHandlerFactory handlerFactory = new();

        IEnumerable<(Type, Func<object, object, Task>)> handlers = handlerFactory.GetTypeHandlers(hubType);

        foreach ((Type handlerType, Func<object, object, Task> handler) in handlers)
        {
            registrar.RegisterHandler(handlerType, obj =>
            {
                object? hubInstance = services.GetService(hubType) ?? throw new InvalidOperationException($"Could not create an instance of {hubType.Name}");

                return handler(hubInstance, obj);
            });
        }
    }
    public static void RegisterTransientHub(this IEventRegistrar registrar, Type hubType, IServiceProvider services)
    {
        EventHubHandlerFactory handlerFactory = new();

        IEnumerable<(Type, Func<object, object, Task>)> handlers = handlerFactory.GetTypeHandlers(hubType);

        foreach ((Type handlerType, Func<object, object, Task> handler) in handlers)
        {
            registrar.RegisterHandler(handlerType, obj =>
            {
                object? hubInstance = ActivatorUtilities.CreateInstance(services, hubType) ?? throw new InvalidOperationException($"Could not create an instance of {hubType.Name}");

                return handler(hubInstance, obj);
            });
        }
    }
    public static void RegisterTransientHub<THub>(this IEventRegistrar registrar, IServiceProvider services) => RegisterTransientHub(registrar, typeof(THub), services);
    public static void RegisterScopedHub(this IEventRegistrar registrar, Type hubType, IServiceProvider services)
    {
        EventHubHandlerFactory handlerFactory = new();

        IEnumerable<(Type, Func<object, object, Task>)> handlers = handlerFactory.GetTypeHandlers(hubType);

        foreach ((Type handlerType, Func<object, object, Task> handler) in handlers)
        {
            registrar.RegisterHandler(handlerType, async obj =>
            {
                IServiceScope scope = services.CreateScope();

                object hubInstance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, hubType) ?? throw new InvalidOperationException($"Could not create an instance of {hubType.Name}");

                try
                {
                    await handler(hubInstance, obj);
                }
                finally
                {
                    scope.Dispose();
                }
            });
        }
    }
    public static void RegisterScopedHub<THub>(this IEventRegistrar registrar, IServiceProvider serviceProvider) => RegisterScopedHub(registrar, typeof(THub), serviceProvider);

}
