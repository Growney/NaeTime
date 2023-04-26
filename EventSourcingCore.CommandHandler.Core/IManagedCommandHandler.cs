using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core
{
    public interface IManagedCommandHandler
    {
        IEnumerable<ICommandHandler> Handlers { get; }
    }
}
