using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.CommandHandler.Abstractions
{
    public class CommandHandlerContainerAttribute : Attribute
    {
        public CommandHandlerScope Scope { get; }
        public CommandHandlerContainerAttribute(CommandHandlerScope scope)
        {
            Scope = scope;
        }
    }
}
