using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Reflection.Abstractions;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core
{
    public class StandardFuncMethodCommandHandler : ICommandHandler
    {
        public string Identifier { get; }

        public IEnumerable<ICommandPrecursor> Precursors { get; }

        public Type CommandType { get; }

        private readonly IStandardFuncMethod<ICommand, CommandMetadata, Task> _standardMethod;

        public StandardFuncMethodCommandHandler(string identifier, IEnumerable<ICommandPrecursor> precursors, IStandardFuncMethod<ICommand, CommandMetadata, Task> standardMethod)
        {
            _standardMethod = standardMethod;
            Identifier = identifier;
            Precursors = precursors;
            CommandType = standardMethod.T1Type;
        }

        public Task Invoke(CommandContext context)
        {
            return _standardMethod.Invoke(context.Services, context.Command, context.Metadata);
        }
    }
}
