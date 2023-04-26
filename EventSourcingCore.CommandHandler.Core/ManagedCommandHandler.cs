using System;
using System.Collections.Generic;
using System.Text;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core
{
    public class ManagedCommandHandler : IManagedCommandHandler
    {
        public ManagedCommandHandler(IEnumerable<ICommandHandler> handler)
        {
            Handlers = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public IEnumerable<ICommandHandler> Handlers { get; }
    }
}
