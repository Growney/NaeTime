using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcingCore.CommandHandler.Abstractions
{
    public interface ICommandHandler
    {
        string Identifier { get; }
        IEnumerable<ICommandPrecursor> Precursors { get; }
        Type CommandType { get; }
        Task Invoke(CommandContext context);
    }
}
