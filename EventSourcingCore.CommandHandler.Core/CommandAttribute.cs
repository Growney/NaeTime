using System;

namespace EventSourcingCore.CommandHandler.Core
{
    public class CommandAttribute : Attribute
    {
        public string Identifier { get; }
        public CommandAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }
}
