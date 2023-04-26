using System;

namespace EventSourcingCore.CommandHandler.Core
{
    public class CommandHandlerAttribute : Attribute
    {
        public string Identifier { get; }
        public CommandHandlerAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }
}
