using System;

namespace EventSourcingCore.CommandHandler.Abstractions
{
    public class CommandContext
    {
        public ICommand Command { get; }
        public CommandMetadata Metadata { get; }
        public IServiceProvider Services { get; }
        public CommandContext(CommandMetadata metadata, ICommand command, IServiceProvider services)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Metadata = metadata;
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
        public CommandContext(CommandMetadata metadata, ICommand command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Metadata = metadata;
        }
    }
}
