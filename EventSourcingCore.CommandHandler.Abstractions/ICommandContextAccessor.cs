namespace EventSourcingCore.CommandHandler.Abstractions
{
    public interface ICommandContextAccessor
    {
        CommandContext Context { get; set; }
    }
}
