using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.CommandHandler.Abstractions
{
    public enum CommandHandlerScope
    {
        Singleton,
        Transient
    }
}
