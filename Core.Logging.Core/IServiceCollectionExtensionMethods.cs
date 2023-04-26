using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Logging.Core
{
    public static class IServiceCollectionExtensionMethods
    {
        /// <summary>
        /// Adds a fixed property to every log line output by the applications logger classes
        /// </summary>
        /// <param name="services"></param>
        /// <param name="key">The property key</param>
        /// <param name="value">The propery value</param>
        /// <returns></returns>
        public static IServiceCollection AddStaticGlobalLogProperty(this IServiceCollection services, string key, object value)
        {
            services.AddSingleton<IGlobalLogPropertyProvider>(new StaticLogPropertyProvider(key, value));
            return services;
        }
        /// <summary>
        /// Adds a fixed property to every log line output by the applications logger classes
        /// </summary>
        /// <param name="services"></param>
        /// <param name="minimum">The minimum log level that the property should be added for</param>
        /// <param name="maximum">The maximum log level that the property should be added for </param>
        /// <param name="key">The property key</param>
        /// <param name="value">The property value</param>
        /// <returns></returns>
        public static IServiceCollection AddStaticGlobalLogProperty(this IServiceCollection services, LogLevel minimum, LogLevel maximum, string key, object value)
        {
            services.AddSingleton<IGlobalLogPropertyProvider>(new StaticLogPropertyProvider(minimum, maximum, key, value));
            return services;
        }
        /// <summary>
        /// Adds a dynamic property to every log line output by the application logger classes
        /// </summary>
        /// <param name="services"></param>
        /// <param name="func">The function that should be called to generate the property</param>
        /// <returns></returns>
        public static IServiceCollection AddCalculatedGlobalLogProperty(this IServiceCollection services, Func<KeyValuePair<string, object>> func)
        {
            services.AddTransient<IGlobalLogPropertyProvider>(x =>
            {
                return new CalculatedLogPropertyProvider(func);
            });
            return services;
        }
        /// <summary>
        /// Adds a dynamic property to every log line output by the application logger classes
        /// </summary>
        /// <typeparam name="T1">Dependency injection provided type. Scope relevant to application route</typeparam>
        /// <param name="services"></param>
        /// <param name="func">The function that should be called to generate the property</param>
        /// <returns></returns>
        public static IServiceCollection AddCalculatedGlobalLogProperty<T1>(this IServiceCollection services, Func<T1, KeyValuePair<string, object>> func)
        {
            services.AddTransient<IGlobalLogPropertyProvider>(x =>
            {
                return new CalculatedLogPropertyProvider(() =>
                {
                    var service = x.GetService<T1>();
                    return func(service);
                });
            });
            return services;
        }
        /// <summary>
        /// Adds a dynamic property to every log line output by the application logger classes
        /// </summary>
        /// <typeparam name="T1">Dependency injection provided type. Scope relevant to application route</typeparam>
        /// <typeparam name="T2">Dependency injection provided type. Scope relevant to application route</typeparam>
        /// <param name="services"></param>
        /// <param name="func"The function that should be called to generate the property></param>
        /// <returns></returns>
        public static IServiceCollection AddCalculatedGlobalLogProperty<T1, T2>(this IServiceCollection services, Func<T1, T2, KeyValuePair<string, object>> func)
        {
            services.AddTransient<IGlobalLogPropertyProvider>(x =>
            {
                return new CalculatedLogPropertyProvider(() =>
                {
                    var service1 = x.GetService<T1>();
                    var service2 = x.GetService<T2>();
                    return func(service1, service2);
                });
            });
            return services;
        }
        /// <summary>
        /// Adds a dynamic property to every log line output by the application logger classes
        /// </summary>
        /// <param name="services"></param>
        /// <param name="minimum">The minimum log level that the property should be added for</param>
        /// <param name="maximum">The maximum log level that the property should be added for</param>
        /// <param name="func">The function that should be called to generate the property</param>
        /// <returns></returns>
        public static IServiceCollection AddCalculatedGlobalLogProperty(this IServiceCollection services, LogLevel minimum, LogLevel maximum, Func<KeyValuePair<string, object>> func)
        {
            services.AddTransient<IGlobalLogPropertyProvider>(x =>
            {
                return new CalculatedLogPropertyProvider(minimum, maximum, func);
            });
            return services;
        }
        /// <summary>
        /// Adds a dynamic property to every log line output by the application logger classes
        /// </summary>
        /// <typeparam name="T1">Dependency injection provided type. Scope relevant to application route</typeparam>
        /// <param name="services"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <param name="func">The function that should be called to generate the property</param>
        /// <returns></returns>
        public static IServiceCollection AddCalculatedGlobalLogProperty<T1>(this IServiceCollection services, LogLevel minimum, LogLevel maximum, Func<T1, KeyValuePair<string, object>> func)
        {
            services.AddTransient<IGlobalLogPropertyProvider>(x =>
            {
                return new CalculatedLogPropertyProvider(minimum, maximum, () =>
                  {
                      var service = x.GetService<T1>();
                      return func(service);
                  });
            });
            return services;
        }
        /// <summary>
        /// Adds a dynamic property to every log line output by the application logger classes
        /// </summary>
        /// <typeparam name="T1">Dependency injection provided type. Scope relevant to application route</typeparam>
        /// <typeparam name="T2">Dependency injection provided type. Scope relevant to application route</typeparam>
        /// <param name="services"></param>
        /// <param name="minimum">The minimum log level that the property should be added for</param>
        /// <param name="maximum">The maximum log level that the property should be added for</param>
        /// <param name="func">The function that should be called to generate the property</param>
        /// <returns></returns>
        public static IServiceCollection AddCalculatedGlobalLogProperty<T1, T2>(this IServiceCollection services, LogLevel minimum, LogLevel maximum, Func<T1, T2, KeyValuePair<string, object>> func)
        {
            services.AddTransient<IGlobalLogPropertyProvider>(x =>
            {
                return new CalculatedLogPropertyProvider(minimum, maximum, () =>
                {
                    var service1 = x.GetService<T1>();
                    var service2 = x.GetService<T2>();
                    return func(service1, service2);
                });
            });
            return services;
        }
    }
}
