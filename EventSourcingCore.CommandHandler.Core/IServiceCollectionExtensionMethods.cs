using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Core.Reflection;
using Core.Reflection.Abstractions;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddCommandHandlerDependencies(this IServiceCollection services)
        {
            services.TryAddTransient<IMethodProviderFactory, MethodProviderFactory>();
            return services;
        }
        public static IServiceCollection AddCommandHandlerCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddTransient<IStandardsBasedRegistrar, StandardsBasedRegistrar>();
            serviceCollection.TryAddSingleton<IGlobalPrecursorRegistry, GlobalPrecursorRegistry>();
            serviceCollection.TryAddSingleton<ICommandHandlerRegistry>(serviceProvider =>
            {
                var registry = ActivatorUtilities.CreateInstance<CommandHandlerRegistry>(serviceProvider);

                var handlers = serviceProvider.GetServices<IManagedCommandHandler>();
                if (handlers != null)
                {
                    foreach (var managedHandler in handlers)
                    {
                        foreach (var handler in managedHandler.Handlers)
                        {
                            registry.RegisterHandler(handler);
                        }
                    }
                }

                return registry;
            });

            return serviceCollection;
        }

        public static IServiceCollection AddSingletonCommandHandlerContainer<T>(this IServiceCollection collection)
        {
            collection.AddTransient<IManagedCommandHandler>(serviceProvider =>
            {
                T instance = ActivatorUtilities.CreateInstance<T>(serviceProvider);

                var registrar = serviceProvider.GetService<IStandardsBasedRegistrar>();

                IEnumerable<ICommandHandler> handlers = registrar.CreateObjectHandlers(instance);

                return new ManagedCommandHandler(handlers);
            });

            return collection;
        }
        public static IServiceCollection AddSingletonCommandHandlerContainer(this IServiceCollection collection, object instance)
        {
            collection.AddTransient<IManagedCommandHandler>(serviceProvider =>
            {
                var registrar = serviceProvider.GetService<IStandardsBasedRegistrar>();

                IEnumerable<ICommandHandler> handlers = registrar.CreateObjectHandlers(instance);

                return new ManagedCommandHandler(handlers);
            });

            return collection;
        }
        public static IServiceCollection AddTransientCommandHandlerContainer<T>(this IServiceCollection collection)
        {
            collection.AddTransient<IManagedCommandHandler>(serviceProvider =>
            {
                var registrar = serviceProvider.GetService<IStandardsBasedRegistrar>();

                IEnumerable<ICommandHandler> handlers = registrar.CreateClassHandlers<T>();

                return new ManagedCommandHandler(handlers);
            });

            return collection;

        }
        public static IServiceCollection AddTransientCommandHandlerContainer(this IServiceCollection collection, Type commandHandlerContainerType)
        {
            collection.AddTransient<IManagedCommandHandler>(serviceProvider =>
            {
                var registrar = serviceProvider.GetService<IStandardsBasedRegistrar>();

                IEnumerable<ICommandHandler> handlers = registrar.CreateClassHandlers(commandHandlerContainerType);

                return new ManagedCommandHandler(handlers);
            });

            return collection;
        }
        public static IServiceCollection AddCommandHandlerContainers(this IServiceCollection collection, Assembly assembly)
        {
            var assemblyTypes = assembly.GetTypes();
            foreach (var type in assemblyTypes)
            {
                var handlerAttribute = type.GetCustomAttribute<CommandHandlerContainerAttribute>();
                if (handlerAttribute != null)
                {
                    switch (handlerAttribute.Scope)
                    {
                        case CommandHandlerScope.Singleton:
                            collection.AddSingletonCommandHandlerContainer(type);
                            break;
                        case CommandHandlerScope.Transient:
                            collection.AddTransientCommandHandlerContainer(type);
                            break;
                        default:
                            break;
                    }
                }
            }
            return collection;
        }
        public static IServiceCollection AddCommandHandlerContainers(this IServiceCollection collection)
        {
            var assembly = Assembly.GetCallingAssembly();
            return collection.AddCommandHandlerContainers(assembly);
        }

        public static IServiceCollection AddDefaultCommandHandlerContainers(this IServiceCollection collection)
        {
            collection.AddCommandHandlerDependencies();
            collection.AddCommandHandlerCore();
            var assembly = Assembly.GetCallingAssembly();
            return collection.AddCommandHandlerContainers(assembly);
        }
    }
}
