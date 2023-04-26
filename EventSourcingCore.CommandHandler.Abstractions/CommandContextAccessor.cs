namespace EventSourcingCore.CommandHandler.Abstractions
{
    public class CommandContextAccessor : ICommandContextAccessor
    {
        public CommandContext Context { get; set; }
    }
}
