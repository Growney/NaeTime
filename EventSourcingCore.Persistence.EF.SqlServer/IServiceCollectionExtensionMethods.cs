using EventSourcingCore.Persistence.EF.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using Core.DependencyInjection.Core;
using Core.Security.Abstractions;
using EventSourcingCore.Persistence.EF.Abstractions;
using EventSourcingCore.Projection.Abstractions;

namespace EventSourcingCore.Persistence.EF.SqlServer
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddSystemSqlServerDBContext<TContext>(this IServiceCollection services, string key = "Default", Action<IServiceProvider, IServiceCollection> configureServices = null)
               where TContext : SystemDBContext<TContext>, IProjectionDBContext
        {
            return services.AddProjectionSqlServerDBContext<TContext>(key, (provider, collection) =>
            {
                collection.AddPassthroughService<ICustomerContextAccessor>(provider);

                configureServices?.Invoke(provider, collection);
            });
        }
        public static IServiceCollection AddCustomerSqlServerDBContext<TContext>(this IServiceCollection services, string key = "Default", Action<IServiceProvider, IServiceCollection> configureServices = null)
            where TContext : CustomerDBContext<TContext>, IProjectionDBContext
        {
            return services.AddProjectionSqlServerDBContext<TContext>(key, (provider, collection) =>
            {
                collection.AddPassthroughService<ICustomerContextAccessor>(provider);

                configureServices?.Invoke(provider, collection);
            });
        }
        public static IServiceCollection AddProjectionSqlServerDBContext<TContext>(this IServiceCollection services, string key = "Default", Action<IServiceProvider, IServiceCollection> configureServices = null)
            where TContext : DbContext, IProjectionDBContext
        {
            services.TryAddTransient<IProjectionPositionRepository, EntityFrameworkProjectionPositionRepository>();
            services.TryAddTransient<IProjectionDBContext>(services => services.GetService<TContext>());

            return services.AddSqlServerDBContext<TContext>(key, configureServices);
        }
        public static IServiceCollection AddSqlServerDBContext<TContext>(this IServiceCollection services, string key = "Default", Action<IServiceProvider, IServiceCollection> configureServices = null) where TContext : DbContext
        {

            services.AddOptions<SqlServerConnectionOptions>(key).Configure<IServiceProvider>((options, serviceprovider) =>
            {
                var configuration = serviceprovider.GetService<IConfiguration>();
                if (configuration != null)
                {
                    configuration.GetSection($"SqlServer:{key}").Bind(options);
                }
            });
            services.AddDbContext<TContext>((serviceProvider, contextOptions) =>
            {
                var entityFrameworkServices = new ServiceCollection();
                entityFrameworkServices.AddEntityFrameworkSqlServer();

                configureServices?.Invoke(serviceProvider, entityFrameworkServices);

                var entityFrameworkServiceProvider = entityFrameworkServices.BuildServiceProvider();
                contextOptions.UseInternalServiceProvider(entityFrameworkServiceProvider);

                var optionsSnap = serviceProvider.GetService<IOptionsSnapshot<SqlServerConnectionOptions>>();
                var options = optionsSnap.Get(key);

                var connectionStringBuilder = new SqlConnectionStringBuilder($"SERVER={options.Server};");
                connectionStringBuilder.UserID = options.Username;
                connectionStringBuilder.Password = options.Password;
                connectionStringBuilder.ConnectTimeout = options.Timeout;
                connectionStringBuilder.InitialCatalog = options.DatabaseName;

                contextOptions.UseSqlServer(connectionStringBuilder.ToString());
            });
            return services;
        }
    }
}
