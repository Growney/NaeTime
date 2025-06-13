using EventDbLite.Events;

namespace EventDbLite.Abstractions;

public interface IAsyncHandlerProvider
{
    AsyncHandler? GetHandlerMethod(object handler, string identifier);
}
