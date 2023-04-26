using Microsoft.Extensions.DependencyInjection;
using System;
using Core.OpenIdConnect.Configuration;
using Core.Configuration.Abstractions;

namespace Core.OpenIdConnect.RemoteHost
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddClientOpenIdConnectConfiguration(this IServiceCollection collection, string clientPrefix = "Client")
        {
            return collection.AddClientConfiguration<OpenIdConnectOptions>($"{clientPrefix}:OpenIdConnect", "OpenIdConnect");
        }
    }
}
