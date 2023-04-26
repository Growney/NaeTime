using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using EventSourcingCore.Aggregate.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Event.Core;
using Core.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EventSourcingCore.Aggregate.Core.Factories;

namespace EventSourcingCore.Aggregate.Core
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddAggregateDependencies(this IServiceCollection services)
        {
            services.AddEventCore();
            services.AddTransient<IStreamNameProvider, StreamNameProvider>();
            return services;
        }
        public static IServiceCollection AddAggregateRepositoryCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IAggregateEventHandlerRepository, StandardAggregateEventHandlerRepository>();

            return serviceCollection;
        }
        public static IServiceCollection AddCustomerAggregateRepository(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddAggregateRepositoryCore();
            serviceCollection.AddTransient<ICustomerAggregateRepository, CustomerAggregateRepository>();
            serviceCollection.AddScoped<IEventMetadataFactory<CustomerEventMetadata>, CustomerEventMetadataFactory>();

            return serviceCollection;
        }
        public static IServiceCollection AddSystemAggregateRepository(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddAggregateRepositoryCore();
            serviceCollection.AddTransient<ISystemAggregateRepository, SystemAggregateRepository>();
            serviceCollection.AddScoped<IEventMetadataFactory<SystemEventMetadata>, SystemEventMetadataFactory>();

            return serviceCollection;
        }
        public static IServiceCollection AddStreamAggregateRepository(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddAggregateRepositoryCore();
            serviceCollection.AddTransient<IStreamAggregateRepository, StreamAggregateRepository>();
            return serviceCollection;
        }
        public static IServiceCollection AddDirectEventRepository(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDirectEventRepository, DirectEventRepository>();
            return serviceCollection;
        }
        public static IServiceCollection AddAggregateCore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddAggregateDependencies();
            serviceCollection.AddCustomerAggregateRepository();
            serviceCollection.AddSystemAggregateRepository();
            serviceCollection.AddStreamAggregateRepository();
            serviceCollection.AddDirectEventRepository();
            return serviceCollection;
        }
    }
}
