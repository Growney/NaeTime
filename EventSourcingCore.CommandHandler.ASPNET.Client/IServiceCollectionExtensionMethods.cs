using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.CommandHandler.ASPNET.Client
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddASPNETCommandHandlerClient(this IServiceCollection collection)
        {
            collection.AddOptions<HandlerClientOptions>().Configure<IServiceProvider>((options, provider) =>
            {
                var configuration = provider.GetService<IConfiguration>();
                if (configuration != null)
                {
                    configuration.GetSection("Handler").Bind(options);
                }
            });

            collection.AddHttpClient();

            collection.AddTransient<IHandlerClient, HandlerClient>();

            return collection;
        }
    }
}
