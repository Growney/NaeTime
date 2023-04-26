using EventSourcingCore.CommandHandler.Abstractions;
using EventSourcingCore.CommandHandler.Core.Tests.Implementations;

namespace EventSourcingCore.CommandHandler.Core.Tests.Implementations
{
    public class TestPrecursorAttribute : PrecursorAttribute
    {
        public override ICommandPrecursor GetPrecursor()
        {
            return new CommandPrecursor();
        }
    }
}
