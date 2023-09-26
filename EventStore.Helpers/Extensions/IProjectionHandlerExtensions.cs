using Microsoft.Extensions.DependencyInjection;

namespace EventStore.Helpers;
public static class IProjectionHandlerExtensions
{
    public static void On<T>(this IProjectionHandler handler, Func<T, Task> on)
    {
        handler.On<T>((eventData, metadata) => on(eventData));
    }

    public static IProjectionHandler AddSingletonProjection<TProjection>(this IProjectionHandler handler) where TProjection : IProjection
    {
        var projection = ActivatorUtilities.CreateInstance<TProjection>(handler.Services);

        projection.Configure(handler);

        return handler;
    }

}
