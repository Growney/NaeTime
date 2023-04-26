using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Configuration.Abstractions
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddClientConfiguration<T>(this IServiceCollection services, string path, string name = null, Action<T> configure = null) where T : class
        {
            var builder = services.AddOptions<T>().BindConfiguration(path);
            if (configure != null)
            {
                builder.Configure(configure);
            }
            services.AddTransient<IClientConfigurationProvider, OptionsClientConfigurationProvider<T>>(provider =>
            {
                var options = provider.GetRequiredService<IOptionsSnapshot<T>>();

                return new OptionsClientConfigurationProvider<T>(name ?? typeof(T).Name, options);

            });
            return services;
        }
    }
}
