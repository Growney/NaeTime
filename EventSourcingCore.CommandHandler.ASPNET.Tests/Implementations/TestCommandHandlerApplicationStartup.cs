using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using EventSourcingCore.CommandHandler.Core;

namespace EventSourcingCore.CommandHandler.ASPNET.Tests.Implementations
{
    public class TestCommandHandlerApplicationStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransientCommandHandlerContainer<CommandHandler>();
            services.AddDefaultASPNetCommandRouting();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseASPNetCommandRouting();
        }
    }
}
