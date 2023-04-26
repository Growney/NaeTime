using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventSourcingCore.CommandHandler.Abstractions;
using EventSourcingCore.CommandHandler.Core;

namespace EventSourcingCore.CommandHandler.ASPNET
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddASPNETCommandRoutingDependencies(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddCommandHandlerDependencies();
            serviceCollection.AddCommandHandlerCore();
            return serviceCollection;
        }

        public static IServiceCollection AddASPNetCommandRouting(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IHttpRequestCommandHandler, DefaultHttpRequestCommandHandler>();
            serviceCollection.AddSingleton<WebCommandHandlerMarkerService>();
            serviceCollection.AddScoped<ICommandContextAccessor, CommandContextAccessor>();

            serviceCollection.AddOptions<WebCommandHandlerOptions>().Configure<IServiceProvider>((options, provider) =>
            {
                var configuration = provider.GetService<IConfiguration>();
                if (configuration != null)
                {
                    configuration.GetSection("CommandHandlerOptions").Bind(options);
                }
            });

            return serviceCollection;
        }

        public static IServiceCollection AddDefaultASPNetCommandRouting(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddASPNETCommandRoutingDependencies();
            serviceCollection.AddASPNetCommandRouting();
            return serviceCollection;
        }
    }
}
