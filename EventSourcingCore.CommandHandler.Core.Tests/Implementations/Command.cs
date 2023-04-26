using System;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core.Tests.Implementations
{
    [Command("Command_v1")]
    public class Command : ICommand
    {
        public Guid Guid { get; set; }

    }
}
