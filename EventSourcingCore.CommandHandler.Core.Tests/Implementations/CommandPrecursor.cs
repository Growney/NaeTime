using System.Threading.Tasks;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core.Tests.Implementations
{
    public class CommandPrecursor : ICommandPrecursor
    {

        public Task Invoke(CommandContext context)
        {
            return Task.CompletedTask;
        }
    }
}
