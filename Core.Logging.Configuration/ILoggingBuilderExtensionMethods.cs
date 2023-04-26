using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Logging.Configuration
{
    public static class ILoggingBuilderExtensionMethods
    {
        public static ILoggingBuilder AddConfigurationGlobalLogProperties(this ILoggingBuilder builder)
        {
            builder.Services.AddConfigurationGlobalLogProperties();
            return builder;
        }
    }
}
