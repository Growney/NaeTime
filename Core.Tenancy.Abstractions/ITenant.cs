namespace Core.Tenancy.Abstractions
{
    public interface ITenant
    {
        public string Subdomain { get; }
    }
}
