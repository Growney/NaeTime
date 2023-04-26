using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Security.Abstractions;

namespace Core.Security.Core
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddSecurityAccessors(this IServiceCollection collection)
        {
            collection.AddScoped<ICustomerContextAccessor, CustomerContextAccessor>();
            collection.AddScoped<ISystemContextAccessor, SystemContextAccessor>();
            collection.AddScoped<IUserContextAccessor, UserContextAccessor>();
            return collection;
        }
        public static IServiceCollection AddSecurityCore<TSecurityKeyProvider>(this IServiceCollection collection)
            where TSecurityKeyProvider : class, ISecurityKeyProvider
        {
            collection.AddSecurityAccessors();
            collection.AddTransient<ISecurityKeyProvider, TSecurityKeyProvider>();
            collection.AddScoped<IClaimsCollectionProvider, JWTClaimsCollectionProvider>();
            collection.AddOptions<JWTOptions>().Configure<IServiceProvider>((options, provider) =>
            {
                var configuration = provider.GetService<IConfiguration>();
                if (configuration != null)
                {
                    configuration.GetSection("JWTOptions").Bind(options);
                }
            });
            return collection;
        }
    }
}
