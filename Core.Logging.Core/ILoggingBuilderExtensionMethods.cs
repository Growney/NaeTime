using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Logging.Core
{
    public static class ILoggingBuilderExtensionMethods
    {
        public static ILoggingBuilder AddServiceName(this ILoggingBuilder builder, string name)
        {
            return builder.AddStaticGlobalLogProperty("ServiceName", name);
        }
        public static ILoggingBuilder AddServiceInstance(this ILoggingBuilder builder)
        {
            return builder.AddStaticGlobalLogProperty("ServiceInstance", Guid.NewGuid());
        }
        public static ILoggingBuilder AddStaticGlobalLogProperty(this ILoggingBuilder builder, string key, object value)
        {
            builder.Services.AddStaticGlobalLogProperty(key, value);
            return builder;
        }
        public static ILoggingBuilder AddStaticGlobalLogProperty(this ILoggingBuilder builder, LogLevel minimum, LogLevel maximum, string key, object value)
        {
            builder.Services.AddStaticGlobalLogProperty(minimum, maximum, key, value);
            return builder;
        }

        public static ILoggingBuilder AddCalculatedGlobalLogProperty(this ILoggingBuilder builder, Func<KeyValuePair<string, object>> func)
        {
            builder.Services.AddCalculatedGlobalLogProperty(func);
            return builder;
        }
        public static ILoggingBuilder AddCalculatedGlobalLogProperty<T1>(this ILoggingBuilder builder, Func<T1, KeyValuePair<string, object>> func)
        {
            builder.Services.AddCalculatedGlobalLogProperty(func);
            return builder;
        }
        public static ILoggingBuilder AddCalculatedGlobalLogProperty<T1, T2>(this ILoggingBuilder builder, Func<T1, T2, KeyValuePair<string, object>> func)
        {
            builder.Services.AddCalculatedGlobalLogProperty(func);
            return builder;
        }
        public static ILoggingBuilder AddCalculatedGlobalLogProperty(this ILoggingBuilder builder, LogLevel minimum, LogLevel maximum, Func<KeyValuePair<string, object>> func)
        {
            builder.Services.AddCalculatedGlobalLogProperty(minimum, maximum, func);
            return builder;
        }
        public static ILoggingBuilder AddCalculatedGlobalLogProperty<T1>(this ILoggingBuilder builder, LogLevel minimum, LogLevel maximum, Func<T1, KeyValuePair<string, object>> func)
        {
            builder.Services.AddCalculatedGlobalLogProperty(minimum, maximum, func);
            return builder;
        }

        public static ILoggingBuilder AddCalculatedGlobalLogProperty<T1, T2>(this ILoggingBuilder builder, LogLevel minimum, LogLevel maximum, Func<T1, T2, KeyValuePair<string, object>> func)
        {
            builder.Services.AddCalculatedGlobalLogProperty(minimum, maximum, func);
            return builder;
        }
    }
}
