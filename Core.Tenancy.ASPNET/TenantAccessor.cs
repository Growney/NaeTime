using Microsoft.AspNetCore.Http;
using System;
using Core.Tenancy.Abstractions;
using Core.Tenancy.ASPNET;

namespace Core.Tenancy.ASPNET
{
    public class TenantAccessor : ITenantAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantAccessor(IHttpContextAccessor contextAccessor) => _httpContextAccessor = contextAccessor;
        public ITenant GetCurrentTenant()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var host = context.Request.Host.Host;
                if (host != null)
                {
                    int lastIndex = host.LastIndexOf('.');
                    if (lastIndex >= 0 && lastIndex < host.Length)
                    {
                        var tenantName = host.AsMemory().Slice(0, lastIndex);
                        return new Tenant(tenantName.ToString());
                    }
                }
            }
            return null;
        }
    }
}
