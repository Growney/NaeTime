using System.Threading.Tasks;

namespace EventSourcingCore.CommandHandler.Abstractions
{
    public interface ICommandPrecursor
    {
        Task Invoke(CommandContext context);
    }
}
