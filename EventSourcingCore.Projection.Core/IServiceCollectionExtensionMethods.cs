using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Core.Reflection;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Event.Core;
using EventSourcingCore.Projection.Abstractions;
using EventSourcingCore.Readiness.Abstractions;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Projection.Core
{
    public static class IServiceCollectionExtensionMethods
    {
        private const string c_defaultKey = "Default";
        public static IServiceCollection AddProjectionDependencies(this IServiceCollection collection)
        {
            collection.AddEventCore();

            return collection;
        }
        public static IServiceCollection AddProjectionManagerBase(this IServiceCollection collection)
        {
            collection.TryAddSingleton<IProjectionManagerBuilderProvider>(x =>
            {
                var builders = x.GetServices<IProjectionManagerBuilder>();

                var provider = new ProjectionManagerBuilderProvider();

                foreach (var builder in builders)
                {
                    provider.AddBuilder(builder);
                }

                return provider;
            });
            collection.AddHostedService<ProjectionManagerService>();
            return collection;
        }


        public static IServiceCollection AddProjectionManager(this IServiceCollection collection, string key, string streamName, Action<ProjectionManagerBuilder> construct)
        {
            collection.AddSingleton<IProjectionManagerBuilder>(serviceProvider =>
            {
                var builder = new ProjectionManagerBuilder(key, streamName, serviceProvider);

                construct(builder);

                return builder;
            });

            collection.AddSingleton(serviceProvider =>
            {
                var provider = serviceProvider.GetService<IProjectionManagerBuilderProvider>();
                var builder = provider.GetProjectionManager(key);
                var manager = builder.Build();

                return manager;
            });

            return collection;
        }

        public static IServiceCollection AddProjectionManager(this IServiceCollection collection, string key, Action<ProjectionManagerBuilder> construct)
        {
            return collection.AddProjectionManager(key, null, construct);
        }

        public static IServiceCollection AddDefaultProjectionManager(this IServiceCollection collection, string streamName, Action<ProjectionManagerBuilder> construct)
        {
            return collection.AddProjectionManager(c_defaultKey, streamName, construct);
        }
        public static IServiceCollection AddDefaultProjectionManager(this IServiceCollection collection, Action<ProjectionManagerBuilder> construct)
        {
            return collection.AddProjectionManager(c_defaultKey, construct);
        }
        public static IServiceCollection AddDefaultProjectionCore(this IServiceCollection collection, Action<ProjectionManagerBuilder> construct = null)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return collection.AddDefaultProjectionCore(callingAssembly, construct);
        }
        /// <summary>
        /// Adds the core requirements and projection handler classes in the calling assembly using the default projection key
        /// </summary>
        /// <param name="collection">The service collection to add the dependencies to</param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultProjectionCore(this IServiceCollection collection, string streamName, Action<ProjectionManagerBuilder> construct = null)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return collection.AddDefaultProjectionCore(streamName, callingAssembly, construct);
        }
        public static IServiceCollection AddDefaultProjectionCore(this IServiceCollection collection, Assembly assembly, Action<ProjectionManagerBuilder> construct = null)
        {
            return collection.AddDefaultProjectionCore(null, assembly, construct);
        }
        /// <summary>
        /// Adds the core requirements and projection handler classes in the provided assembly
        /// </summary>
        /// <param name="collection">The service collection to add the dependencies to</param>
        /// <param name="assembly">The assembly that contains the projections</param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultProjectionCore(this IServiceCollection collection, string streamName, Assembly assembly, Action<ProjectionManagerBuilder> construct = null)
        {
            collection.AddProjectionDependencies();
            collection.AddProjectionManagerBase();
            collection.AddDefaultProjectionManager(streamName, x =>
             {
                 construct?.Invoke(x);
                 x.AddAssemblyProjections(assembly);
             });
            return collection;
        }
    }
}
