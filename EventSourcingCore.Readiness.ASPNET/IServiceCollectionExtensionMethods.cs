using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Readiness.ASPNET
{
    public static class IServiceCollectionExtensionMethods
    {

        public static IServiceCollection AddWebReadiness(this IServiceCollection collection)
        {
            collection.AddOptions<WebReadinessOptions>().Configure<IServiceProvider>((options, provider) =>
            {
                var configuration = provider.GetService<IConfiguration>();
                if (configuration != null)
                {
                    configuration.GetSection("Readiness").Bind(options);
                }
            });
            return collection;

        }
    }
}
