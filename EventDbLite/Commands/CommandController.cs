using EventDbLite.Abstractions;
using EventDbLite.Handlers;
using EventDbLite.Handlers.Abstractions;

namespace EventDbLite.Commands;

public class CommandController
{
    internal ICommandSerializer? CommandSerializer { get; set; }
    internal ICommandHandlerProvider? HandlerProvider { get; set; }

    internal Task<bool> Raise(string identifier, object command)
    {
        if (CommandSerializer is null)
        {
            throw new InvalidOperationException("Command serializer must not be null");
        }

        if (HandlerProvider is null)
        {
            throw new InvalidOperationException("Handler provider must not be null");
        }

        CommandHandler? handler = HandlerProvider.GetHandlerMethod(this, identifier)
            ?? throw new InvalidOperationException($"No handler method found for identifier '{identifier}'");

        return handler.Action(command);
    }
}
