using Microsoft.Extensions.DependencyInjection;
using Core.Tenancy.Abstractions;

namespace Core.Tenancy.ASPNET
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddTenancy(this IServiceCollection services)
        {
            services.AddSingleton<ITenantAccessor, TenantAccessor>();
            return services;
        }
    }
}
