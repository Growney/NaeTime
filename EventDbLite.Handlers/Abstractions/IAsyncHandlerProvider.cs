using EventDbLite.Handlers;

namespace EventDbLite.Handlers.Abstractions;

public interface IAsyncHandlerProvider
{
    AsyncHandler? GetHandlerMethod(Type handlerType, string identifier);
    IEnumerable<AsyncHandler> GetHandlerMethods(Type handlerType);
}
