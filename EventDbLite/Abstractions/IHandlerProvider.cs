using EventDbLite.Handlers;

namespace EventDbLite.Abstractions;
public interface IHandlerProvider
{
    Handler? GetHandlerMethod(Type handlerType, string identifier);
    IEnumerable<Handler> GetAllHandlerMethods(Type handlerType);
}