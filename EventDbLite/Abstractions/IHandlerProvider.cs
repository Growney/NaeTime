using EventDbLite.Handlers;

namespace EventDbLite.Abstractions;
public interface IHandlerProvider
{
    Handler? GetHandlerMethod(object handler, string identifier);
}