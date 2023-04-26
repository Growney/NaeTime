namespace Core.Tenancy.Abstractions
{
    public interface ITenantAccessor
    {
        ITenant GetCurrentTenant();
    }
}
