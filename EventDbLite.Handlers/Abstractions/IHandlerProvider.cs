namespace EventDbLite.Handlers.Abstractions;
public interface IHandlerProvider
{
    Handler? GetHandlerMethod(Type handlerType, string identifier);
    IEnumerable<Handler> GetAllHandlerMethods(Type handlerType);
}