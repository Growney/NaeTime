using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Security.Abstractions;
using Core.Security.Core;

namespace Core.Security.ASPNET
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddASPNETSecurityCore<SecurityKeyProvider>(this IServiceCollection collection) where SecurityKeyProvider : class, ISecurityKeyProvider
        {
            collection.AddSecurityCore<SecurityKeyProvider>();
            collection.AddOptions<JWTHeaderOptions>().Configure<IServiceProvider>((options, provider) =>
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
