using EventDbLite.Handlers;

namespace EventDbLite.Abstractions;

public interface IAsyncHandlerProvider
{
    AsyncHandler? GetHandlerMethod(Type handlerType, string identifier);
    IEnumerable<AsyncHandler> GetHandlerMethods(Type handlerType);
}
