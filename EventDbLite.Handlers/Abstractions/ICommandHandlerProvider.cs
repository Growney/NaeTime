using EventDbLite.Handlers;

namespace EventDbLite.Handlers.Abstractions;

public interface ICommandHandlerProvider
{
    CommandHandler? GetHandlerMethod(object handler, string identifier);
}
