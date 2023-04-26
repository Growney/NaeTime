using System.Collections.Generic;

namespace EventSourcingCore.CommandHandler.Abstractions
{
    public interface ICommandHandlerRegistry
    {
        IEnumerable<string> RegisteredIdentifiers { get; }
        ICommandHandler GetHandler(string identifier);
        void RegisterHandler(ICommandHandler handler);
    }
}
