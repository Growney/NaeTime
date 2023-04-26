using Core.Tenancy.Abstractions;

namespace Core.Tenancy.ASPNET
{
    public class Tenant : ITenant
    {
        public string Subdomain { get; }
        public Tenant(string subdomain)
        {
            Subdomain = subdomain;
        }

    }
}
