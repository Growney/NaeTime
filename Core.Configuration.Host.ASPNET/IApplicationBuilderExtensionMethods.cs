using Core.Configuration.Host.ASPNET;
using Microsoft.AspNetCore.Builder;
using System;
using Core.Configuration.Abstractions;

namespace Core.Configuration.Host.ASPNET
{
    public static class IApplicationBuilderExtensionMethods
    {
        public static IApplicationBuilder UseClientConfigurationProvider(this IApplicationBuilder builder) => builder.UseClientConfigurationProvider(ConfigurationConstants.DefaultPath);
        public static IApplicationBuilder UseClientConfigurationProvider(this IApplicationBuilder builder, string path) => builder.UseMiddleware<ClientConfigurationProviderMiddleware>(path);
    }
}
