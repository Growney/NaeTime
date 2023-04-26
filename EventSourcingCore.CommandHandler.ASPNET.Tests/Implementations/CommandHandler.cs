using System.Threading.Tasks;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.ASPNET.Tests.Implementations
{
    [CommandHandlerContainer(CommandHandlerScope.Transient)]
    public class CommandHandler
    {

        public Task Handle(Command command)
        {
            return Task.CompletedTask;
        }
    }
}
