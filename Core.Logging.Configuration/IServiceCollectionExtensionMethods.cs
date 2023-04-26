using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Logging.Core;

namespace Core.Logging.Configuration
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddConfigurationGlobalLogProperties(this IServiceCollection services)
        {
            services.AddSingleton<IGlobalLogPropertyProvider, ConfigurationGlobalLogPropertyProvider>();
            return services;
        }
    }
}
