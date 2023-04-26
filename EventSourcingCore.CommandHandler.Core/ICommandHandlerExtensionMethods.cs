using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core
{
    public static class ICommandHandlerExtensionMethods
    {
        public static async Task Execute(this ICommandHandler handler, CommandContext commandContext)
        {
            ICommandContextAccessor accessor = commandContext.Services.GetService<ICommandContextAccessor>();
            if (accessor != null)
            {
                accessor.Context = commandContext;
            }
            else
            {

            }

            if (handler.Precursors != null)
            {
                foreach (ICommandPrecursor precursor in handler.Precursors)
                {
                    await precursor.Invoke(commandContext);
                }
            }

            await handler.Invoke(commandContext);

        }
        public static Task ExecuteScoped(this ICommandHandler handler, CommandContext context)
        {
            using (IServiceScope commandExecutionScope = context.Services.CreateScope())
            {
                CommandContext scopedContext = new CommandContext(context.Metadata, context.Command, commandExecutionScope.ServiceProvider);
                return handler.Execute(scopedContext);
            }
        }
    }
}
