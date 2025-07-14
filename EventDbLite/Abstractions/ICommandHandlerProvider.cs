using EventDbLite.Handlers;

namespace EventDbLite.Abstractions;

public interface ICommandHandlerProvider
{
    CommandHandler? GetHandlerMethod(object handler, string identifier);
}
